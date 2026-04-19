using Messaging.Contracts.Common;
using UserService.DTOs;

namespace UserService.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<IEnumerable<UserProfileResponse>>> GetAllUserAsync();
        Task<ApiResponse<UserProfileResponse>> GetUserAsync(Guid id);
        Task<ApiResponse<ConfirmationResponse>> DeleteUserAsync(Guid id);
        Task<ApiResponse<ConfirmationResponse>> UpdateUserProfileAsync(Guid id, UserProfileUpdateRequest request);
    }
}
