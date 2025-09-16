using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Services.Implementation
{
    public class SqsPaymentConsumerService : BackgroundService
    {
        private readonly ILogger<SqsPaymentConsumerService> _logger;
        private readonly IAmazonSQS _sqsClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly SqsOptions _options;

        public SqsPaymentConsumerService(
            ILogger<SqsPaymentConsumerService> logger,
            IAmazonSQS sqsClient,
            IServiceProvider serviceProvider,
            IOptions<SqsOptions> options)
        {
            _logger = logger;
            _sqsClient = sqsClient;
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SQS Payment Consumer starting. QueueUrl={QueueUrl}", _options.QueueUrl);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var receiveRequest = new ReceiveMessageRequest
                    {
                        QueueUrl = _options.QueueUrl,
                        MaxNumberOfMessages = 5,
                        WaitTimeSeconds = 20,
                        MessageAttributeNames = new List<string> { "All" }
                    };

                    var response = await _sqsClient.ReceiveMessageAsync(receiveRequest, stoppingToken);
                    if (response.Messages == null || !response.Messages.Any())
                    {
                        continue;
                    }

                    foreach (var msg in response.Messages)
                    {
                        if (stoppingToken.IsCancellationRequested) break;

                        try
                        {
                            // Example: SQS may contain an SNS envelope where the actual payload is in the "Message" field.
                            Console.WriteLine("Processing SQS message: " + msg.Body);

                            string? innerJson = null;
                            try
                            {
                                using var outerDoc = JsonDocument.Parse(msg.Body);
                                if (outerDoc.RootElement.ValueKind == JsonValueKind.Object && outerDoc.RootElement.TryGetProperty("Message", out var inner))
                                {
                                    // SNS delivers Message as a JSON string (quoted). If so, extract the string value and
                                    // parse it to get the real JSON payload. Otherwise, Message may already be an object.
                                    if (inner.ValueKind == JsonValueKind.String)
                                    {
                                        var messageString = inner.GetString();
                                        if (!string.IsNullOrEmpty(messageString))
                                        {
                                            // messageString is itself a JSON string; use it directly as the inner JSON
                                            innerJson = messageString;
                                        }
                                    }
                                    else
                                    {
                                        innerJson = inner.GetRawText();
                                    }
                                }
                            }
                            catch (JsonException)
                            {
                                // not JSON or malformed; we'll fallback to raw body
                            }

                            Console.WriteLine("Inner JSON: " + (innerJson ?? msg.Body));

                            var jsonToParse = innerJson ?? msg.Body;
                            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var paymentEvent = JsonSerializer.Deserialize<PaymentEventDto>(jsonToParse, jsonOptions);

                            // If some fields are missing from the inner JSON, try to fall back to MessageAttributes (SNS often includes metadata there)
                            if (paymentEvent != null && (string.IsNullOrWhiteSpace(paymentEvent.Id) || string.IsNullOrWhiteSpace(paymentEvent.ExternalId) || string.IsNullOrWhiteSpace(paymentEvent.Status)))
                            {
                                try
                                {
                                    var attrMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                    if (msg.MessageAttributes != null)
                                    {
                                        foreach (var kv in msg.MessageAttributes)
                                        {
                                            var key = kv.Key ?? string.Empty;
                                            var normalized = key.Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();

                                            // Prefer StringValue when available, otherwise decode BinaryValue as UTF8
                                            var value = kv.Value?.StringValue;
                                            if (string.IsNullOrEmpty(value) && kv.Value?.BinaryValue != null)
                                            {
                                                try
                                                {
                                                    var bytes = kv.Value.BinaryValue.ToArray();
                                                    value = System.Text.Encoding.UTF8.GetString(bytes);
                                                }
                                                catch
                                                {
                                                    // ignore binary decoding problems
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(value))
                                            {
                                                attrMap[normalized] = value;
                                            }
                                        }
                                    }

                                    // Map likely attribute names to DTO fields if they are missing
                                    if (string.IsNullOrWhiteSpace(paymentEvent.Id))
                                    {
                                        if (attrMap.TryGetValue("id", out var v)) paymentEvent.Id = v;
                                    }
                                    if (string.IsNullOrWhiteSpace(paymentEvent.ExternalId))
                                    {
                                        if (attrMap.TryGetValue("externalid", out var v)) paymentEvent.ExternalId = v;
                                    }
                                    if (string.IsNullOrWhiteSpace(paymentEvent.Status))
                                    {
                                        if (attrMap.TryGetValue("status", out var v)) paymentEvent.Status = v;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogDebug(ex, "Error while reading MessageAttributes fallback");
                                }
                            }

                            if (paymentEvent == null)
                            {
                                _logger.LogWarning("Received SQS message with empty body or invalid format. MessageId={MessageId}", msg.MessageId);
                                // Delete malformed messages to avoid retry storms
                                await _sqsClient.DeleteMessageAsync(_options.QueueUrl, msg.ReceiptHandle, stoppingToken);
                                continue;
                            }

                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var paymentHandler = scope.ServiceProvider.GetService<IPaymentEventHandler>();
                                if (paymentHandler == null)
                                {
                                    _logger.LogError("No IPaymentEventHandler registered; skipping message {MessageId}", msg.MessageId);
                                }
                                else
                                {
                                    await paymentHandler.HandleAsync(paymentEvent, stoppingToken);
                                }
                            }

                            // Delete message after successful processing
                            await _sqsClient.DeleteMessageAsync(_options.QueueUrl, msg.ReceiptHandle, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing SQS message {MessageId}", msg.MessageId);
                            // Do not delete the message — it will become visible again for retries or dead-letter routing
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Shutdown requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SQS polling error");
                    // Backoff on error
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            _logger.LogInformation("SQS Payment Consumer stopping");
        }
    }
}
