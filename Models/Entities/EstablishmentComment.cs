using System.ComponentModel.DataAnnotations;
using VisitaBookingApi.Models;

namespace visita_booking_api.Models.Entities
{
    public class EstablishmentComment
    {
        public int Id { get; set; }

        public int EstablishmentId { get; set; }
        public Establishment Establishment { get; set; } = null!;

        public int AdminId { get; set; }
        public User Admin { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
