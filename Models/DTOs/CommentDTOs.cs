namespace visita_booking_api.Models.DTOs
{
    /// <summary>
    /// Request to add an internal admin comment to an accommodation
    /// </summary>
    public class AddCommentRequest
    {
        public string Comment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to set accommodation to pending with optional internal comment
    /// </summary>
    public class SetPendingRequest
    {
        public string? Comment { get; set; }
    }

    /// <summary>
    /// Response DTO for admin comments (internal use only)
    /// </summary>
    public class AccommodationCommentDto
    {
        public int Id { get; set; }
        public int AccommodationId { get; set; }
        public int AdminId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}