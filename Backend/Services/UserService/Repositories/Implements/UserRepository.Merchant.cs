using Microsoft.EntityFrameworkCore;
using UserService.Entities;
using UserService.Enums;

namespace UserService.Repositories.Implements
{
    public partial class UserRepository
    {
        public async Task<bool> CreateMerchantRequest(MerchantRequest merchantRequest)
        {
            await _context.MerchantRequests.AddAsync(merchantRequest);

            var result = await _context.SaveChangesAsync();

            if (result == 0)
                return false;
            return true;
        }

        public async Task<IQueryable<MerchantRequest>> GetAllMerchantRequestAsync()
        {
            return _context.MerchantRequests
                .AsNoTracking()
                .OrderByDescending(mr => mr.CreatedAt);
        }

        public async Task<MerchantRequest?> GetMerchantRequestByIdAsync(Guid requestId)
        {
            return await _context.MerchantRequests
                .FirstOrDefaultAsync(mr => mr.Id == requestId);
        }

        public async Task<MerchantRequest?> GetPendingMerchantRequestByUserIdAsync(Guid userId)
        {
            return await _context.MerchantRequests
                .FirstOrDefaultAsync(mr => mr.UserId == userId && mr.VerificationStatus == VerificationStatus.Pending);
        }

        public async Task UpdateMerchantRequestAsync(MerchantRequest merchantRequest)
        {
            _context.MerchantRequests.Update(merchantRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CreateMerchantAsync(Merchant merchant)
        {
            await _context.Merchants.AddAsync(merchant);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<IQueryable<Merchant>> GetAllMerchantsAsync()
        {
            return _context.Merchants
                .AsNoTracking()
                .Where(m => m.DeletedAt == null)
                .OrderByDescending(m => m.CreatedAt);
        }

        public async Task<Merchant?> GetMerchantByIdAsync(Guid merchantId)
        {
            return await _context.Merchants
                .FirstOrDefaultAsync(m => m.Id == merchantId && m.DeletedAt == null);
        }

        public async Task<Merchant?> GetMerchantByUserIdAsync(Guid userId)
        {
            return await _context.Merchants
                .FirstOrDefaultAsync(m => m.UserId == userId && m.DeletedAt == null);
        }

        public async Task UpdateMerchantAsync(Merchant merchant)
        {
            _context.Merchants.Update(merchant);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteMerchantAsync(Guid merchantId)
        {
            var merchant = await _context.Merchants
                .FirstOrDefaultAsync(m => m.Id == merchantId && m.DeletedAt == null);

            if (merchant == null)
                return false;

            merchant.DeletedAt = DateTime.UtcNow;
            merchant.Status = MerchantStatus.Rejected;
            merchant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
