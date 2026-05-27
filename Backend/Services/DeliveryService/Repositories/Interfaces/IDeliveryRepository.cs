using DeliveryService.Entities;
using DeliveryService.Repositories;

namespace DeliveryService.Repositories.Interfaces
{
    public interface IDeliveryRepository
    {
        Task CreateShipperAvailabilityAsync(ShipperAvailability shipperAvailability);
        Task<IQueryable<ShipperAvailability>> GetAllShipperAvailabilityAsync();
        Task UpdateShipperAvailabilityAsync(ShipperAvailability shipperAvailability);
        Task<ShipperAvailability?> GetShipperAvailabilityByShipperIdAsync(Guid shipperId);
        Task AddShipperLocationHistoriesAsync(IEnumerable<ShipperLocationHistory> histories, CancellationToken cancellationToken = default);
        Task<IQueryable<ShipperLocationHistory>> GetAllShipperLocationHistoriesByOrderIdAsync(Guid orderId);
        Task<IQueryable<ShipperLocationHistory>> GetAllShipperLocationHistoriesByShipperIdAsync(Guid shipperId);

        Task<IQueryable<ShipperAssignment>> GetAllShipperAssignmentsAsync();
        Task<ShipperAssignment?> GetShipperAssignmentByIdAsync(Guid assignmentId);
        Task<IQueryable<ShipperAssignment>> GetAllShipperAssignmentsByOrderIdAsync(Guid orderId);
        Task<IQueryable<ShipperAssignment>> GetAllShipperAssignmentsByShipperIdAsync(Guid shipperId);
        Task<ShipperAssignment?> GetAcceptedShipperAssignmentByOrderIdAsync(Guid orderId);
        Task UpdateShipperAssignment(ShipperAssignment shipperAssignment);
        Task CreateShipperAssignment(ShipperAssignment shipperAssignment);
        Task CreateShipperAssignmentsAsync(IEnumerable<ShipperAssignment> shipperAssignments);
        Task<bool> TryCreateAssignmentOfferAsync(ShipperAssignment shipperAssignment, DateTime expiresAt);
        Task<AssignmentAcceptanceResult> AcceptAssignmentOfferAsync(Guid assignmentId, Guid shipperId, DateTime now);
        Task<ShipperAssignment?> RejectAssignmentOfferAsync(Guid assignmentId, Guid shipperId, string reason, DateTime now);
        Task<IReadOnlyList<ShipperAssignment>> ExpireStaleAssignmentOffersAsync(DateTime now, CancellationToken cancellationToken = default);
        Task<ShipperAssignment?> GetActiveOfferForShipperAsync(Guid shipperId);

        Task CreateIncidentAsync(Incident incident);
        Task<IQueryable<Incident>> GetAllIncidentsAsync();
        Task<IQueryable<Incident>> GetAllIncidentByReporterId(Guid reporterId);
        Task<Incident?> GetIncidentByIdAsync(Guid incidentId);
        Task UpdateIncidentAsync(Incident incident);
    }
}
