using DeliveryService.Entities;
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

        public async Task AddShipperLocationHistoriesAsync(IEnumerable<ShipperLocationHistory> histories, CancellationToken cancellationToken = default)
        {
            await _context.ShipperLocationHistories.AddRangeAsync(histories, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
