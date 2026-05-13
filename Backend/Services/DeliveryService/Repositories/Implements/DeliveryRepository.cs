using DeliveryService.Entities;
using DeliveryService.Enums;
using DeliveryService.Persistences;
using DeliveryService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Repositories.Implements
{
    public class DeliveryRepository : IDeliveryRepository
    {
        private readonly DeliveryDbContext _context;

        public DeliveryRepository(DeliveryDbContext context)
        {
            _context = context;
        }

        public async Task CreateShipperAvailabilityAsync(ShipperAvailability shipperAvailability)
        {
            await _context.ShipperAvailabilities.AddAsync(shipperAvailability);
            await _context.SaveChangesAsync();
        }

        public Task<IQueryable<ShipperAvailability>> GetAllShipperAvailabilityAsync()
        {
            IQueryable<ShipperAvailability> query = _context.ShipperAvailabilities
                .AsNoTracking()
                .Where(sa => sa.DeletedAt == null);

            return Task.FromResult(query);
        }

        public async Task<ShipperAvailability?> GetShipperAvailabilityByShipperIdAsync(Guid shipperId)
        {
            return await _context.ShipperAvailabilities.FirstOrDefaultAsync(sa => sa.ShipperId == shipperId && sa.DeletedAt == null);
        }

        public async Task UpdateShipperAvailabilityAsync(ShipperAvailability shipperAvailability)
        {
            _context.ShipperAvailabilities.Update(shipperAvailability);
            await _context.SaveChangesAsync();
        }

        public async Task CreateShipperAssignment(ShipperAssignment shipperAssignment)
        {
            await _context.ShipperAssignments.AddAsync(shipperAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task CreateShipperAssignmentsAsync(IEnumerable<ShipperAssignment> shipperAssignments)
        {
            await _context.ShipperAssignments.AddRangeAsync(shipperAssignments);
            await _context.SaveChangesAsync();
        }

        public Task<IQueryable<ShipperAssignment>> GetAllShipperAssignmentsAsync()
        {
            IQueryable<ShipperAssignment> query = _context.ShipperAssignments
                .AsNoTracking()
                .Where(sa => sa.Status != AssignmentStatus.Cancelled)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public async Task<ShipperAssignment?> GetShipperAssignmentByIdAsync(Guid assignmentId)
        {
            return await _context.ShipperAssignments.FirstOrDefaultAsync(sa => sa.Id == assignmentId);
        }

        public Task<IQueryable<ShipperAssignment>> GetAllShipperAssignmentsByOrderIdAsync(Guid orderId)
        {
            IQueryable<ShipperAssignment> query = _context.ShipperAssignments
                .AsNoTracking()
                .Where(sa => sa.OrderId == orderId)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public Task<IQueryable<ShipperAssignment>> GetAllShipperAssignmentsByShipperIdAsync(Guid shipperId)
        {
            IQueryable<ShipperAssignment> query = _context.ShipperAssignments
                .AsNoTracking()
                .Where(sa => sa.ShipperId == shipperId)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public async Task<ShipperAssignment?> GetAcceptedShipperAssignmentByOrderIdAsync(Guid orderId)
        {
            return await _context.ShipperAssignments.FirstOrDefaultAsync(sa => sa.OrderId == orderId && sa.Status == AssignmentStatus.Accepted);
        }

        public async Task UpdateShipperAssignment(ShipperAssignment shipperAssignment)
        {
            _context.ShipperAssignments.Update(shipperAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task AddShipperLocationHistoriesAsync(IEnumerable<ShipperLocationHistory> histories, CancellationToken cancellationToken = default)
        {
            await _context.ShipperLocationHistories.AddRangeAsync(histories, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task CreateIncidentAsync(Incident incident)
        {
            await _context.Incidents.AddAsync(incident);
            await _context.SaveChangesAsync();
        }

        public Task<IQueryable<Incident?>> GetAllIncidentsAsync()
        {
            IQueryable<Incident?> query = _context.Incidents
                .AsNoTracking()
                .Where(i => i.DeletedAt == null)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public Task<IQueryable<Incident?>> GetAllIncidentByReporterId(Guid reporterId)
        {
            IQueryable<Incident?> query = _context.Incidents
                .AsNoTracking()
                .Where(i => i.ReportedBy == reporterId && i.DeletedAt == null)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public async Task<Incident?> GetIncidentByReporterIdAsync(Guid incidentId)
        {
            return await _context.Incidents.FirstOrDefaultAsync(i => i.Id == incidentId && i.DeletedAt == null);
        }
    }
}
