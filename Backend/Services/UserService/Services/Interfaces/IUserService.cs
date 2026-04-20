using Messaging.Contracts.Common;
using UserService.DTOs;

namespace UserService.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<PagedResult<UserProfileResponse>>> GetAllUserAsync(PaginationRequest pagedOption);
        Task<ApiResponse<UserProfileResponse>> GetUserAsync(Guid id);
        Task<ApiResponse<ConfirmationResponse>> DeleteUserAsync(Guid id);
        Task<ApiResponse<ConfirmationResponse>> UpdateUserProfileAsync(Guid id, UserProfileUpdateRequest request);
        Task<ApiResponse<ConfirmationResponse>> RequestForMerchantRole(Guid userId, CreateMerchantRequest request);
        Task<ApiResponse<PagedResult<MerchantRequestResponse>>> GetAllMerchantRequests(PaginationRequest pagedOption);
        Task<ApiResponse<ConfirmationResponse>> ReviewMerchantRequestAsync(Guid requestId, Guid reviewerId, ReviewMerchantRequest request);
        Task<ApiResponse<PagedResult<MerchantResponse>>> GetAllMerchantsAsync(PaginationRequest pagedOption);
        Task<ApiResponse<MerchantResponse>> GetMerchantByIdAsync(Guid merchantId);
        Task<ApiResponse<ConfirmationResponse>> UpdateMerchantAsync(Guid merchantId, UpdateMerchantRequest request);
        Task<ApiResponse<ConfirmationResponse>> DeleteMerchantAsync(Guid merchantId);
    }
}
