using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace visita_booking_api.Models.Entities
{
    public enum PaymentMethod
    {
        CreditCard = 1,
        DebitCard = 2,
        BankTransfer = 3,
        EWallet = 4,
        QRCode = 5,
        Cash = 6
    }

    public enum PaymentType
    {
        FullPayment = 1,      // Full payment for booking
        PartialPayment = 2,   // Partial payment (e.g., deposit)
        Refund = 3,          // Refund payment
        Fee = 4,             // Additional fee
        Adjustment = 5       // Price adjustment
    }

    /// <summary>
    /// Represents a payment transaction for a booking
    /// </summary>
    public class BookingPayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string PaymentReference { get; set; } = string.Empty;

        // Foreign Keys
        public int BookingId { get; set; }

        // Payment Details
        public PaymentType PaymentType { get; set; } = PaymentType.FullPayment;
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        // Xendit Integration
        [StringLength(100)]
        public string? XenditInvoiceId { get; set; }

        [StringLength(100)]
        public string? XenditPaymentId { get; set; }

        [StringLength(100)]
        public string? XenditExternalId { get; set; }

        [StringLength(500)]
        public string? XenditPaymentUrl { get; set; }

        // Payment Processing
        public DateTime? ProcessedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? FailedAt { get; set; }

        [StringLength(1000)]
        public string? FailureReason { get; set; }

        // Provider Details
        [StringLength(100)]
        public string? ProviderTransactionId { get; set; }

        [StringLength(100)]
        public string? ProviderPaymentMethod { get; set; }

        [StringLength(4)]
        public string? CardLastFour { get; set; }

        [StringLength(50)]
        public string? BankCode { get; set; }

        // Fees
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ProviderFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PlatformFee { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal NetAmount { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Refund Information
        public int? RefundedFromPaymentId { get; set; }
        
        [StringLength(500)]
        public string? RefundReason { get; set; }

        // JSON fields for storing provider-specific data
        [Column(TypeName = "json")]
        public string? XenditWebhookData { get; set; }

        [Column(TypeName = "json")]
        public string? ProviderMetadata { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(BookingId))]
        public virtual Booking Booking { get; set; } = null!;

        [ForeignKey(nameof(RefundedFromPaymentId))]
        public virtual BookingPayment? RefundedFromPayment { get; set; }

        public virtual ICollection<BookingPayment> RefundPayments { get; set; } = new List<BookingPayment>();

        // Computed Properties
        [NotMapped]
        public bool IsSuccessful => Status == PaymentStatus.Paid;

        [NotMapped]
        public bool IsPending => Status == PaymentStatus.Pending || Status == PaymentStatus.Processing;

        [NotMapped]
        public bool IsRefund => PaymentType == PaymentType.Refund;

        [NotMapped]
        public string PaymentDescription => PaymentType switch
        {
            PaymentType.FullPayment => "Full Payment",
            PaymentType.PartialPayment => "Partial Payment",
            PaymentType.Refund => "Refund",
            PaymentType.Fee => "Additional Fee",
            PaymentType.Adjustment => "Price Adjustment",
            _ => "Payment"
        };
    }
}