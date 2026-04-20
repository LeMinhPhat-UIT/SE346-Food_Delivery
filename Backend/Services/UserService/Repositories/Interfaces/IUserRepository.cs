using UserService.Entities;

namespace UserService.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IQueryable<User>> GetAllUserAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task CreateUserAsync(User user);
        Task<bool> DeleteUserAsync(Guid id);
        Task UpdateUserAsync(User user);
        Task<bool> CreateMerchantRequest(MerchantRequest merchantRequest);
        Task<IQueryable<MerchantRequest>> GetAllMerchantRequestAsync();
        Task<MerchantRequest?> GetMerchantRequestByIdAsync(Guid requestId);
        Task<MerchantRequest?> GetPendingMerchantRequestByUserIdAsync(Guid userId);
        Task UpdateMerchantRequestAsync(MerchantRequest merchantRequest);

        Task<bool> CreateMerchantAsync(Merchant merchant);
        Task<IQueryable<Merchant>> GetAllMerchantsAsync();
        Task<Merchant?> GetMerchantByIdAsync(Guid merchantId);
        Task<Merchant?> GetMerchantByUserIdAsync(Guid userId);
        Task UpdateMerchantAsync(Merchant merchant);
        Task<bool> DeleteMerchantAsync(Guid merchantId);
    }
}
