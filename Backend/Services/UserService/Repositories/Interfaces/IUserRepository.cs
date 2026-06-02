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

        Task<bool> CreateUserAddressAsync(Address address);
        Task<IQueryable<Address>> GetAllUserAddressesAsync();
        Task<IQueryable<Address>?> GetAllUserAddressesByUserIdAsync(Guid userId);
        Task<Address?> GetUserAddressByIdAsync(Guid addressId);
        Task UpdateUserAddressAsync(Address address);
        Task<bool> DeleteUserAddressAsync(Guid addressId);

        Task<bool> CreateMerchantRequest(MerchantRequest merchantRequest);
        Task<IQueryable<MerchantRequest>> GetAllMerchantRequestAsync();
        Task<MerchantRequest?> GetMerchantRequestByIdAsync(Guid requestId);
        Task<MerchantRequest?> GetPendingMerchantRequestByUserIdAsync(Guid userId);
        Task<MerchantRequest?> GetLatestMerchantRequestByUserIdAsync(Guid userId);
        Task UpdateMerchantRequestAsync(MerchantRequest merchantRequest);

        Task<bool> CreateMerchantAsync(Merchant merchant);
        Task<IQueryable<Merchant>> GetAllMerchantsAsync();
        Task<Merchant?> GetMerchantByIdAsync(Guid merchantId);
        Task<IQueryable<MerchantAddress>?> GetMerchantAddressesByMerchantIdAsync(Guid merchantId);
        Task<MerchantAddress?> GetMerchantAddressByIdAsync(Guid addressId);
        Task<bool> CreateMerchantAddressAsync(MerchantAddress merchantAddress);
        Task UpdateMerchantAddressAsync(MerchantAddress merchantAddress);
        Task<bool> DeleteMerchantAddressAsync(Guid addressId);
        Task<Merchant?> GetMerchantByUserIdAsync(Guid userId);
        Task UpdateMerchantAsync(Merchant merchant);
        Task<bool> DeleteMerchantAsync(Guid merchantId);

        Task<bool> CreateShipperRequest(ShipperRequest shipperRequest);
        Task<IQueryable<ShipperRequest>> GetAllShipperRequestAsync();
        Task<ShipperRequest?> GetShipperRequestByIdAsync(Guid requestId);
        Task<ShipperRequest?> GetPendingShipperRequestByUserIdAsync(Guid userId);
        Task<ShipperRequest?> GetLatestShipperRequestByUserIdAsync(Guid userId);
        Task UpdateShipperRequestAsync(ShipperRequest shipperRequest);

        Task<bool> CreateShipperAsync(Shipper shipper);
        Task<IQueryable<Shipper>> GetAllShippersAsync();
        Task<Shipper?> GetShipperByIdAsync(Guid shipperId);
        Task<Shipper?> GetShipperByUserIdAsync(Guid userId);
        Task UpdateShipperAsync(Shipper shipper);
        Task<bool> DeleteShipperAsync(Guid shipperId);
    }
}
