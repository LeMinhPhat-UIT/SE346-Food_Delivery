using DeliveryService.Entities;

namespace DeliveryService.Repositories
{
    public enum AssignmentAcceptanceOutcome
    {
        Accepted,
        NotFound,
        OfferNotFound,
        OfferExpired,
        AlreadyTaken,
        NotAvailable
    }

    public sealed class AssignmentAcceptanceResult
    {
        public AssignmentAcceptanceOutcome Outcome { get; init; }
        public ShipperAssignment? Assignment { get; init; }
        public IReadOnlyList<ShipperAssignment> CancelledAssignments { get; init; } = Array.Empty<ShipperAssignment>();

        public static AssignmentAcceptanceResult FromOutcome(AssignmentAcceptanceOutcome outcome, ShipperAssignment? assignment = null)
        {
            return new AssignmentAcceptanceResult
            {
                Outcome = outcome,
                Assignment = assignment
            };
        }
    }
}
