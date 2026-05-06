using Messaging.Contracts.Common;
using Messaging.Contracts.Extensions;
using NotificationService.DTOs;
using NotificationService.Entities;
using NotificationService.Mappers;
using NotificationService.Repositories.Interfaces;
using NotificationService.Services.Interfaces;

namespace NotificationService.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly UserDeviceMapper _mapper;

        public NotificationService(INotificationRepository repository, UserDeviceMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedResult<UserDeviceResponse>>> GetAllUserDevicesAysnc(PaginationRequest paginationRequest)
        {
            var userDevices = await _repository.GetAllUserDevicesAsync();

            if (userDevices == null || userDevices.Count() == 0)
                return new ApiResponse<PagedResult<UserDeviceResponse>>(StatusCodes.Status404NotFound, "No user devices found");

            var pagedUserDevices = await userDevices.ToPagedResultAsync(paginationRequest);
            var response = _mapper.ToUserDeviceResponseList(pagedUserDevices.Items);

            return new ApiResponse<PagedResult<UserDeviceResponse>>(StatusCodes.Status200OK, new PagedResult<UserDeviceResponse>(response));
        }

        public async Task<ApiResponse<PagedResult<UserDeviceResponse>>> GetAllUserDevicesByUserIdAsync(Guid userId, PaginationRequest paginationRequest)
        {
            if (userId == Guid.Empty)
                return new ApiResponse<PagedResult<UserDeviceResponse>>(StatusCodes.Status400BadRequest, "Invalid user id");

            var userDevices = await _repository.GetAllUserDevicesByUserIdAsync(userId);

            if (userDevices == null || userDevices.Count() == 0)
                return new ApiResponse<PagedResult<UserDeviceResponse>>(StatusCodes.Status404NotFound, "No user devices found");

            var pagedUserDevices = await userDevices.ToPagedResultAsync(paginationRequest);
            var response = _mapper.ToUserDeviceResponseList(pagedUserDevices.Items);

            return new ApiResponse<PagedResult<UserDeviceResponse>>(StatusCodes.Status200OK, new PagedResult<UserDeviceResponse>(response));
        }
    }
}
