using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Services.Implementation
{
    public class XenditService : IXenditService
    {
        private readonly HttpClient _httpClient;
        private readonly XenditConfiguration _config;
        private readonly ILogger<XenditService> _logger;

        public XenditService(
            HttpClient httpClient,
            IOptions<XenditConfiguration> config,
            ILogger<XenditService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;

            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ApiKey}:")));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "VisitaBookingAPI/1.0");
        }

        public async Task<XenditInvoiceResult> CreateInvoiceAsync(Booking booking, int expiryMinutes = 30)
        {
            try
            {
                var externalId = $"booking-{booking.BookingReference}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                var expiryDate = DateTime.UtcNow.AddMinutes(expiryMinutes);

                var invoiceRequest = new
                {
                    external_id = externalId,
                    amount = booking.TotalAmount,
                    description = $"Booking Payment for {booking.Room?.Name ?? "Room"} - {booking.BookingReference}",
                    invoice_duration = expiryMinutes * 60, // Convert to seconds
                    currency = "USD", // TODO: Make configurable
                    reminder_time = 1, // 1 hour before expiry
                    customer = new
                    {
                        given_names = booking.GuestName.Split(' ').FirstOrDefault() ?? "",
                        surname = booking.GuestName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                        email = booking.GuestEmail,
                        mobile_number = booking.GuestPhone ?? ""
                    },
                    customer_notification_preference = new
                    {
                        invoice_created = new[] { "email" },
                        invoice_reminder = new[] { "email" },
                        invoice_paid = new[] { "email" },
                        invoice_expired = new[] { "email" }
                    },
                    success_redirect_url = $"{_config.SuccessRedirectUrl}?booking={booking.BookingReference}",
                    failure_redirect_url = $"{_config.FailureRedirectUrl}?booking={booking.BookingReference}",
                    metadata = new
                    {
                        booking_id = booking.Id,
                        booking_reference = booking.BookingReference,
                        room_id = booking.RoomId,
                        user_id = booking.UserId,
                        check_in_date = booking.CheckInDate.ToString("yyyy-MM-dd"),
                        check_out_date = booking.CheckOutDate.ToString("yyyy-MM-dd")
                    }
                };

                var json = JsonSerializer.Serialize(invoiceRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/v2/invoices", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var invoiceResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    return new XenditInvoiceResult
                    {
                        IsSuccess = true,
                        InvoiceId = invoiceResponse.GetProperty("id").GetString(),
                        PaymentUrl = invoiceResponse.GetProperty("invoice_url").GetString(),
                        ExternalId = externalId,
                        ExpiryDate = expiryDate,
                        Amount = booking.TotalAmount,
                        Currency = "USD",
                        Status = invoiceResponse.GetProperty("status").GetString() ?? "",
                        RawResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent) ?? new()
                    };
                }
                else
                {
                    _logger.LogError("Failed to create Xendit invoice. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseContent);

                    return new XenditInvoiceResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Failed to create invoice: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating Xendit invoice for booking {BookingId}",
                    booking.Id);

                return new XenditInvoiceResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<XenditInvoiceStatus> GetInvoiceStatusAsync(string invoiceId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v2/invoices/{invoiceId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var invoiceData = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    return new XenditInvoiceStatus
                    {
                        InvoiceId = invoiceData.GetProperty("id").GetString() ?? "",
                        ExternalId = invoiceData.GetProperty("external_id").GetString() ?? "",
                        Status = invoiceData.GetProperty("status").GetString() ?? "",
                        Amount = invoiceData.GetProperty("amount").GetDecimal(),
                        PaidAmount = invoiceData.TryGetProperty("paid_amount", out var paidAmount) ? paidAmount.GetDecimal() : null,
                        Currency = invoiceData.GetProperty("currency").GetString() ?? "",
                        PaidAt = invoiceData.TryGetProperty("paid_at", out var paidAt) && paidAt.ValueKind != JsonValueKind.Null
                            ? DateTime.Parse(paidAt.GetString() ?? "") : null,
                        ExpiryDate = invoiceData.TryGetProperty("expiry_date", out var expiryDate) && expiryDate.ValueKind != JsonValueKind.Null
                            ? DateTime.Parse(expiryDate.GetString() ?? "") : null,
                        PaymentMethod = invoiceData.TryGetProperty("payment_method", out var paymentMethod) 
                            ? paymentMethod.GetString() : null,
                        PaymentChannel = invoiceData.TryGetProperty("payment_channel", out var paymentChannel) 
                            ? paymentChannel.GetString() : null,
                        BankCode = invoiceData.TryGetProperty("bank_code", out var bankCode) 
                            ? bankCode.GetString() : null,
                        RawData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent) ?? new()
                    };
                }
                else
                {
                    _logger.LogError("Failed to get Xendit invoice status. InvoiceId: {InvoiceId}, Status: {StatusCode}",
                        invoiceId, response.StatusCode);

                    throw new InvalidOperationException($"Failed to get invoice status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting Xendit invoice status for {InvoiceId}", invoiceId);
                throw;
            }
        }

        public async Task<bool> ExpireInvoiceAsync(string invoiceId)
        {
            try
            {
                var expireRequest = new { };
                var json = JsonSerializer.Serialize(expireRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/v2/invoices/{invoiceId}/expire", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully expired Xendit invoice {InvoiceId}", invoiceId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to expire Xendit invoice {InvoiceId}. Status: {StatusCode}",
                        invoiceId, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while expiring Xendit invoice {InvoiceId}", invoiceId);
                return false;
            }
        }

        public async Task<XenditWebhookResult> ProcessWebhookAsync(string webhookData, string signature)
        {
            try
            {
                // Validate webhook signature
                if (!ValidateWebhookSignature(webhookData, signature))
                {
                    return new XenditWebhookResult
                    {
                        IsValid = false,
                        ErrorMessage = "Invalid webhook signature"
                    };
                }

                var webhookPayload = JsonSerializer.Deserialize<JsonElement>(webhookData);
                var eventType = webhookPayload.GetProperty("event").GetString() ?? "";

                var result = new XenditWebhookResult
                {
                    IsValid = true,
                    EventType = eventType,
                    WebhookData = JsonSerializer.Deserialize<Dictionary<string, object>>(webhookData) ?? new()
                };

                // Extract common fields
                if (webhookPayload.TryGetProperty("id", out var id))
                    result.InvoiceId = id.GetString();

                if (webhookPayload.TryGetProperty("external_id", out var externalId))
                    result.ExternalId = externalId.GetString();

                if (webhookPayload.TryGetProperty("status", out var status))
                    result.Status = status.GetString() ?? "";

                if (webhookPayload.TryGetProperty("amount", out var amount))
                    result.Amount = amount.GetDecimal();

                if (webhookPayload.TryGetProperty("paid_at", out var paidAt) && paidAt.ValueKind != JsonValueKind.Null)
                    result.PaidAt = DateTime.Parse(paidAt.GetString() ?? "");

                // Map payment method
                if (webhookPayload.TryGetProperty("payment_method", out var paymentMethod))
                {
                    result.PaymentMethod = MapXenditPaymentMethod(paymentMethod.GetString());
                }

                _logger.LogInformation("Processed Xendit webhook: EventType={EventType}, Status={Status}, ExternalId={ExternalId}",
                    eventType, result.Status, result.ExternalId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while processing Xendit webhook");
                
                return new XenditWebhookResult
                {
                    IsValid = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public bool ValidateWebhookSignature(string payload, string signature)
        {
            try
            {
                if (string.IsNullOrEmpty(_config.WebhookVerificationToken))
                {
                    _logger.LogWarning("Xendit webhook verification token not configured");
                    return false;
                }

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.WebhookVerificationToken));
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var computedSignature = Convert.ToHexString(computedHash).ToLower();

                return signature.Equals(computedSignature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while validating webhook signature");
                return false;
            }
        }

        public async Task<XenditRefundResult> CreateRefundAsync(string paymentId, decimal amount, string reason)
        {
            try
            {
                var refundRequest = new
                {
                    amount = amount,
                    reason = reason,
                    metadata = new
                    {
                        refund_initiated_at = DateTime.UtcNow.ToString("O"),
                        refund_reason = reason
                    }
                };

                var json = JsonSerializer.Serialize(refundRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/credit_card_charges/{paymentId}/refunds", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var refundResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    return new XenditRefundResult
                    {
                        IsSuccess = true,
                        RefundId = refundResponse.GetProperty("id").GetString(),
                        PaymentId = paymentId,
                        RefundAmount = amount,
                        Status = refundResponse.GetProperty("status").GetString() ?? "",
                        ProcessedAt = DateTime.UtcNow,
                        Reason = reason,
                        RawResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent) ?? new()
                    };
                }
                else
                {
                    _logger.LogError("Failed to create Xendit refund. PaymentId: {PaymentId}, Status: {StatusCode}",
                        paymentId, response.StatusCode);

                    return new XenditRefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Failed to create refund: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating Xendit refund for payment {PaymentId}", paymentId);

                return new XenditRefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<List<XenditPaymentMethod>> GetAvailablePaymentMethodsAsync(string currency = "USD", decimal? amount = null)
        {
            try
            {
                var queryParams = $"currency={currency}";
                if (amount.HasValue)
                    queryParams += $"&amount={amount.Value}";

                var response = await _httpClient.GetAsync($"/payment_methods?{queryParams}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var paymentMethods = JsonSerializer.Deserialize<JsonElement[]>(responseContent);
                    var result = new List<XenditPaymentMethod>();

                    foreach (var method in paymentMethods ?? Array.Empty<JsonElement>())
                    {
                        result.Add(new XenditPaymentMethod
                        {
                            Type = method.GetProperty("type").GetString() ?? "",
                            Name = method.GetProperty("name").GetString() ?? "",
                            Code = method.TryGetProperty("code", out var code) ? code.GetString() ?? "" : "",
                            IsActive = method.TryGetProperty("is_active", out var isActive) && isActive.GetBoolean(),
                            MinAmount = method.TryGetProperty("min_amount", out var minAmount) ? minAmount.GetDecimal() : null,
                            MaxAmount = method.TryGetProperty("max_amount", out var maxAmount) ? maxAmount.GetDecimal() : null,
                            SupportedCurrencies = method.TryGetProperty("supported_currencies", out var currencies) 
                                ? currencies.EnumerateArray().Select(c => c.GetString() ?? "").ToList() 
                                : new List<string>()
                        });
                    }

                    return result;
                }
                else
                {
                    _logger.LogError("Failed to get available payment methods. Status: {StatusCode}", response.StatusCode);
                    return new List<XenditPaymentMethod>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting available payment methods");
                return new List<XenditPaymentMethod>();
            }
        }

        public async Task<XenditBalanceInfo> GetBalanceAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/balance");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var balanceData = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    return new XenditBalanceInfo
                    {
                        Balance = balanceData.GetProperty("balance").GetDecimal(),
                        Currency = balanceData.GetProperty("currency").GetString() ?? "",
                        LastUpdated = DateTime.UtcNow,
                        CurrencyBalances = balanceData.TryGetProperty("currency_balances", out var currencyBalances)
                            ? JsonSerializer.Deserialize<Dictionary<string, decimal>>(currencyBalances.GetRawText()) ?? new()
                            : new()
                    };
                }
                else
                {
                    throw new InvalidOperationException($"Failed to get balance: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting Xendit balance");
                throw;
            }
        }

        private PaymentMethod? MapXenditPaymentMethod(string? xenditMethod)
        {
            return xenditMethod?.ToLowerInvariant() switch
            {
                "credit_card" => PaymentMethod.CreditCard,
                "debit_card" => PaymentMethod.DebitCard,
                "bank_transfer" => PaymentMethod.BankTransfer,
                "ewallet" => PaymentMethod.EWallet,
                "qr_code" => PaymentMethod.QRCode,
                _ => null
            };
        }
    }

    public class XenditConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.xendit.co";
        public string WebhookVerificationToken { get; set; } = string.Empty;
        public string SuccessRedirectUrl { get; set; } = string.Empty;
        public string FailureRedirectUrl { get; set; } = string.Empty;
        public bool UseProduction { get; set; } = false;
        public int DefaultInvoiceExpiryMinutes { get; set; } = 30;
        public decimal MaxRefundAmount { get; set; } = 10000;
        public bool EnableWebhookValidation { get; set; } = true;
    }
}