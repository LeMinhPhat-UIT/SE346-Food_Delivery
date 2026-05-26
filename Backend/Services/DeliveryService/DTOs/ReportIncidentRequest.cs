using DeliveryService.Enums;

namespace DeliveryService.DTOs
{
    public class ReportIncidentRequest
    {
        public Guid OrderId { get; set; }
        public IncidentType Type { get; set; }
        public string Description { get; set; } = null!;
        public List<string> ProofUrls { get; set; } = new();
    }
}
