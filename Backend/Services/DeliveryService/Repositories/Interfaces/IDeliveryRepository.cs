using DeliveryService.Entities;

namespace DeliveryService.Repositories.Interfaces
{
    public interface IDeliveryRepository
    {
        Task CreateShipperAvailabilityAsync(ShipperAvailability shipperAvailability);
        Task<IQueryable<ShipperAvailability>> GetAllShipperAvailabilityAsync();
        Task UpdateShipperAvailabilityAsync(ShipperAvailability shipperAvailability);
        Task<ShipperAvailability?> GetShipperAvailabilityByShipperIdAsync(Guid shipperId);
        Task AddShipperLocationHistoriesAsync(IEnumerable<ShipperLocationHistory> histories, CancellationToken cancellationToken = default);

        Task<IQueryable<ShipperAssignment>> GetAllShipperAssignmentsAsync();
        Task<ShipperAssignment?> GetShipperAssignmentByIdAsync(Guid assignmentId);
        Task<IQueryable<ShipperAssignment>> GetAllShipperAssignmentsByOrderIdAsync(Guid orderId);
        Task<IQueryable<ShipperAssignment>> GetAllShipperAssignmentsByShipperIdAsync(Guid shipperId);
        Task<ShipperAssignment?> GetAcceptedShipperAssignmentByOrderIdAsync(Guid orderId);
        Task UpdateShipperAssignment(ShipperAssignment shipperAssignment);
        Task CreateShipperAssignment(ShipperAssignment shipperAssignment);
        Task CreateShipperAssignmentsAsync(IEnumerable<ShipperAssignment> shipperAssignments);

        Task CreateIncidentAsync(Incident incident);
        Task<IQueryable<Incident?>> GetAllIncidentsAsync();
        Task<IQueryable<Incident?>> GetAllIncidentByReporterId(Guid reporterId);
        Task<Incident?> GetIncidentByReporterIdAsync(Guid incidentId);

    }
}
