using Messaging.Contracts.Common;
using NotificationService.DTOs;

namespace NotificationService.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ApiResponse<PagedResult<UserDeviceResponse>>> GetAllUserDevicesAysnc(PaginationRequest paginationRequest);
        Task<ApiResponse<PagedResult<UserDeviceResponse>>> GetAllUserDevicesByUserIdAsync(Guid userId, PaginationRequest paginationRequest);
    }
}
