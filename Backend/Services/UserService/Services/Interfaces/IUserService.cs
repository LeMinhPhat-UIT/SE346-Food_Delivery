using Messaging.Contracts.Common;
using UserService.DTOs.MerchantDTOs;
using UserService.DTOs.ShipperDTOs;
using UserService.DTOs.User;

namespace UserService.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<PagedResult<UserProfileResponse>>> GetAllUserAsync(PaginationRequest pagedOption);
        Task<ApiResponse<UserProfileResponse>> GetUserAsync(Guid id);
        Task<ApiResponse<ConfirmationResponse>> DeleteUserAsync(Guid id);
        Task<ApiResponse<ConfirmationResponse>> UpdateUserProfileAsync(Guid id, UpdateUserProfileRequest request);

        Task<ApiResponse<PagedResult<UserAddressResponse>>> GetAllUserAddressesAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<PagedResult<UserAddressResponse>>> GetAllUserAddressesByUserIdAsync(Guid userId, PaginationRequest paginationRequest);
        Task<ApiResponse<UserAddressResponse>> GetUserAddressByIdAsync(Guid addressId);
        Task<ApiResponse<ConfirmationResponse>> AddUserAddressAsync(Guid id, CreateUserAddressRequest request);
        Task<ApiResponse<ConfirmationResponse>> UpdateUserAddressAsync(Guid addressId, UpdateUserAddressRequest request);
        Task<ApiResponse<ConfirmationResponse>> DeleteUserAddressAsync(Guid addressId);

        Task<ApiResponse<ConfirmationResponse>> RequestForMerchantRole(Guid userId, CreateMerchantRequest request);
        Task<ApiResponse<PagedResult<MerchantRequestResponse>>> GetAllMerchantRequestsAsync(PaginationRequest pagedOption);
        Task<ApiResponse<ConfirmationResponse>> ReviewMerchantRequestAsync(Guid requestId, Guid reviewerId, ReviewMerchantRequest request);
        Task<ApiResponse<PagedResult<MerchantResponse>>> GetAllMerchantsAsync(PaginationRequest pagedOption);
        Task<ApiResponse<MerchantResponse>> GetMerchantByIdAsync(Guid merchantId);
        Task<ApiResponse<PagedResult<MerchantAddressResponse>>> GetMerchantAddressesByMerchantIdAsync(PaginationRequest paginationRequest, Guid merchantId);
        Task<ApiResponse<MerchantAddressResponse>> GetMerchantAddressByIdAsync(Guid addressId);
        Task<ApiResponse<ConfirmationResponse>> AddMerchantAddressAsync(Guid merchantId, CreateMerchantAddressRequest request);
        Task<ApiResponse<ConfirmationResponse>> UpdateMerchantAddressAsync(Guid addressId, UpdateMerchantAddressRequest request);
        Task<ApiResponse<ConfirmationResponse>> DeleteMerchantAddressAsync(Guid addressId);
        Task<ApiResponse<ConfirmationResponse>> UpdateMerchantAsync(Guid merchantId, UpdateMerchantRequest request);
        Task<ApiResponse<ConfirmationResponse>> DeleteMerchantAsync(Guid merchantId);

        Task<ApiResponse<ConfirmationResponse>> RequestForShipperRole(Guid userId, CreateShipperRequest request);
        Task<ApiResponse<PagedResult<ShipperRequestResponse>>> GetAllShipperRequestsAsync(PaginationRequest pagedOption);
        Task<ApiResponse<ShipperResponse>> GetShipperByIdAsync(Guid shipperId);
        Task<ApiResponse<ConfirmationResponse>> ReviewShipperRequestAsync(Guid requestId, Guid reviewerId, ReviewShipperRequest request);
        Task<ApiResponse<PagedResult<ShipperResponse>>> GetAllShippersAsync(PaginationRequest pagedOption);
        Task<ApiResponse<ConfirmationResponse>> UpdateShipperAsync(Guid shipperId, UpdateShipperRequest request);
        Task<ApiResponse<ConfirmationResponse>> DeleteShipperAsync(Guid shipperId);
    }
}
