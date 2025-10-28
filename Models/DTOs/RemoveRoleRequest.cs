namespace VisitaBookingApi.Models.DTOs
{
    public class RemoveRoleRequest
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}