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
                            // Example: assume message body is JSON representing a payment event
                            var paymentEvent = JsonSerializer.Deserialize<PaymentEventDto>(msg.Body);

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
