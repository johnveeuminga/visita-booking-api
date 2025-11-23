namespace visita_booking_api.Models.Entities
{
    public class EstablishmentHours
    {
        public int Id { get; set; }

        public int EstablishmentId { get; set; }
        public Establishment Establishment { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }

        public bool IsClosed { get; set; } = false;
    }
}
