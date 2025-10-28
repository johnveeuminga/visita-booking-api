using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VisitaBookingApi.Models;  // ADD THIS LINE

namespace visita_booking_api.Models.Entities
{
    /// <summary>
    /// Represents an internal admin note/comment on an accommodation.
    /// These comments are ONLY visible to admins and NOT to accommodation owners.
    /// Used for internal communication between admin staff.
    /// </summary>
    public class AccommodationComment
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The accommodation this comment belongs to
        /// </summary>
        [Required]
        public int AccommodationId { get; set; }

        [ForeignKey(nameof(AccommodationId))]
        public Accommodation Accommodation { get; set; } = null!;

        /// <summary>
        /// The admin who wrote this comment
        /// </summary>
        [Required]
        public int AdminId { get; set; }

        [ForeignKey(nameof(AdminId))]
        public User Admin { get; set; } = null!;

        /// <summary>
        /// The internal admin note/comment
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// When the comment was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}