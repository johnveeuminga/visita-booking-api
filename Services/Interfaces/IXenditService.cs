using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Services.Interfaces
{
    public interface IXenditService
    {
        /// <summary>
        /// Creates a Xendit invoice for a booking
        /// </summary>
        /// <param name="booking">The booking to create invoice for</param>
        /// <param name="expiryMinutes">Invoice expiry time in minutes</param>
        /// <returns>Invoice creation result with payment URL</returns>
        Task<XenditInvoiceResult> CreateInvoiceAsync(Booking booking, int expiryMinutes = 30);

        /// <summary>
        /// Creates a Xendit invoice with specific expiry time
        /// </summary>
        /// <param name="booking">The booking to create invoice for</param>
        /// <param name="expiryDateTime">Exact expiry date and time</param>
        /// <returns>Invoice creation result with payment URL</returns>
        Task<XenditInvoiceResult> CreateInvoiceAsync(Booking booking, DateTime expiryDateTime);

        /// <summary>
        /// Retrieves invoice status from Xendit
        /// </summary>
        /// <param name="invoiceId">Xendit invoice ID</param>
        /// <returns>Current invoice status</returns>
        Task<XenditInvoiceStatus> GetInvoiceStatusAsync(string invoiceId);

        /// <summary>
        /// Expires a Xendit invoice
        /// </summary>
        /// <param name="invoiceId">Xendit invoice ID</param>
        /// <returns>Success status</returns>
        Task<bool> ExpireInvoiceAsync(string invoiceId);

        /// <summary>
        /// Processes Xendit webhook data
        /// </summary>
        /// <param name="webhookData">Raw webhook data</param>
        /// <param name="signature">Xendit webhook signature for verification</param>
        /// <returns>Processed webhook result</returns>
        Task<XenditWebhookResult> ProcessWebhookAsync(string webhookData, string signature);

        /// <summary>
        /// Validates Xendit webhook signature
        /// </summary>
        /// <param name="payload">Webhook payload</param>
        /// <param name="signature">Webhook signature</param>
        /// <returns>True if signature is valid</returns>
        bool ValidateWebhookSignature(string payload, string signature);

        /// <summary>
        /// Initiates a refund for a payment
        /// </summary>
        /// <param name="paymentId">Xendit payment ID</param>
        /// <param name="amount">Refund amount</param>
        /// <param name="reason">Refund reason</param>
        /// <returns>Refund result</returns>
        Task<XenditRefundResult> CreateRefundAsync(string paymentId, decimal amount, string reason);

        /// <summary>
        /// Gets available payment methods for a customer
        /// </summary>
        /// <param name="currency">Currency code</param>
        /// <param name="amount">Payment amount</param>
        /// <returns>Available payment methods</returns>
        Task<List<XenditPaymentMethod>> GetAvailablePaymentMethodsAsync(string currency = "USD", decimal? amount = null);

        /// <summary>
        /// Gets Xendit balance for monitoring
        /// </summary>
        /// <returns>Account balance information</returns>
        Task<XenditBalanceInfo> GetBalanceAsync();
    }

    #region Xendit Response Models

    public class XenditInvoiceResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? InvoiceId { get; set; }
        public string? PaymentUrl { get; set; }
        public string? ExternalId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Dictionary<string, object> RawResponse { get; set; } = new();
    }

    public class XenditInvoiceStatus
    {
        public string InvoiceId { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal? PaidAmount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentChannel { get; set; }
        public string? BankCode { get; set; }
        public Dictionary<string, object> RawData { get; set; } = new();
    }

    public class XenditWebhookResult
    {
        public bool IsValid { get; set; }
        public bool IsProcessed { get; set; }
        public string? ErrorMessage { get; set; }
        public string? InvoiceId { get; set; }
        public string? ExternalId { get; set; }
        public string? PaymentId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public Dictionary<string, object> WebhookData { get; set; } = new();
    }

    public class XenditRefundResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RefundId { get; set; }
        public string? PaymentId { get; set; }
        public decimal RefundAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public string? Reason { get; set; }
        public Dictionary<string, object> RawResponse { get; set; } = new();
    }

    public class XenditPaymentMethod
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public List<string> SupportedCurrencies { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class XenditBalanceInfo
    {
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public Dictionary<string, decimal> CurrencyBalances { get; set; } = new();
    }

    #endregion
}