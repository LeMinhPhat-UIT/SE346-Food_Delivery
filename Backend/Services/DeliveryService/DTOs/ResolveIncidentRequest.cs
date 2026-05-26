using DeliveryService.Enums;

namespace DeliveryService.DTOs
{
    public class ResolveIncidentRequest
    {
        public IncidentStatus Status { get; set; } = IncidentStatus.Resolved;
        public string Resolution { get; set; } = null!;
    }
}
