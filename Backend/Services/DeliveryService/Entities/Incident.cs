using DeliveryService.Commons;
using DeliveryService.Enums;

namespace DeliveryService.Entities
{
    public class Incident : BaseAuditableEntity
    {
        public Guid OrderId { get; set; }
        public Guid ReportedBy { get; set; }
        public IncidentType Type { get; set; }
        public string Description { get; set; } = null!;
        public IEnumerable<string> ProofUrl { get; set; } = null!;
        public IncidentStatus Status { get; set; } = IncidentStatus.Pending;
        public string Resolution { get; set; } = null!;
        public Guid? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
