using System.Text.Json.Serialization;

namespace visita_booking_api.Services.Implementation
{
    public class PaymentEventDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        // Add other fields as required by your payment event shape
    }
}
