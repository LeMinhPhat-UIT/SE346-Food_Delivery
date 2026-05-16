using Microsoft.EntityFrameworkCore;
using UserService.Entities;
using UserService.Enums;

namespace UserService.Repositories.Implements
{
    public partial class UserRepository
    {
        public async Task<bool> CreateShipperRequest(ShipperRequest shipperRequest)
        {
            await _context.ShipperRequests.AddAsync(shipperRequest);

            var result = await _context.SaveChangesAsync();

            return result != 0;
        }

        public async Task<IQueryable<ShipperRequest>> GetAllShipperRequestAsync()
        {
            return _context.ShipperRequests
                .AsNoTracking()
                .OrderByDescending(sr => sr.CreatedAt);
        }

        public async Task<ShipperRequest?> GetShipperRequestByIdAsync(Guid requestId)
        {
            return await _context.ShipperRequests.FirstOrDefaultAsync(sr => sr.Id == requestId);
        }

        public async Task<ShipperRequest?> GetPendingShipperRequestByUserIdAsync(Guid userId)
        {
            return await _context.ShipperRequests
                .FirstOrDefaultAsync(sr => sr.UserId == userId && sr.VerificationStatus == Enums.VerificationStatus.Pending);
        }

        public async Task UpdateShipperRequestAsync(ShipperRequest shipperRequest)
        {
            _context.ShipperRequests.Update(shipperRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CreateShipperAsync(Shipper shipper)
        {
            await _context.Shippers.AddAsync(shipper);

            var result = await _context.SaveChangesAsync();

            return result != 0;
        }

        public async Task<IQueryable<Shipper>> GetAllShippersAsync()
        {
            return _context.Shippers
                .AsNoTracking()
                .Where(s => s.DeletedAt == null)
                .OrderByDescending(s => s.CreatedAt);
        }

        public async Task<Shipper?> GetShipperByIdAsync(Guid shipperId)
        {
            return await _context.Shippers
                .FirstOrDefaultAsync(s => s.Id == shipperId && s.DeletedAt == null);
        }

        public async Task<Shipper?> GetShipperByUserIdAsync(Guid userId)
        {
            return await _context.Shippers
                .FirstOrDefaultAsync(s => s.UserId == userId && s.DeletedAt == null);
        }

        public async Task UpdateShipperAsync(Shipper shipper)
        {
            _context.Shippers.Update(shipper);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteShipperAsync(Guid shipperId)
        {
            var shipper = await _context.Shippers.FirstOrDefaultAsync(s => s.Id == shipperId && s.DeletedAt == null);

            if (shipper == null)
                return false;

            shipper.DeletedAt = DateTime.UtcNow;
            shipper.Status = ShipperStatus.Rejected;
            shipper.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
