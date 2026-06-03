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
        private readonly UserDeviceMapper _userDeviceMapper;
        private readonly NotificationMapper _notificationMapper;

        public NotificationService(
            INotificationRepository repository,
            UserDeviceMapper userDeviceMapper,
            NotificationMapper notificationMapper)
        {
            _repository = repository;
            _userDeviceMapper = userDeviceMapper;
            _notificationMapper = notificationMapper;
        }

        public async Task<ApiResponse<PagedResult<NotificationResponse>>> GetAllNotificationsByUserIdAsync(Guid userId, PaginationRequest paginationRequest)
        {
            if (userId == Guid.Empty)
                return new ApiResponse<PagedResult<NotificationResponse>>(StatusCodes.Status400BadRequest, "Invalid user id");

            var notifications = await _repository.GetAllNotificationsByUserIdAsync(userId);
            var pagedNotifications = await notifications.ToPagedResultAsync(paginationRequest);

            if (pagedNotifications.TotalCount == 0)
                return new ApiResponse<PagedResult<NotificationResponse>>(StatusCodes.Status404NotFound, "No notifications found");

            var response = _notificationMapper.ToNotificationResponseList(pagedNotifications.Items);
            var result = new PagedResult<NotificationResponse>(response, pagedNotifications.PaginationRequest, pagedNotifications.TotalCount);

            return new ApiResponse<PagedResult<NotificationResponse>>(StatusCodes.Status200OK, result);
        }

        public async Task<ApiResponse<ConfirmationResponse>> MarkNotificationAsReadAsync(Guid userId, Guid notificationId)
        {
            if (userId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid user id");

            if (notificationId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid notification id");

            var notification = await _repository.GetNotificationByIdAsync(notificationId);

            if (notification == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No notification found");

            if (notification.UserId != userId)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status403Forbidden, "You can only update your own notifications");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _repository.UpdateNotificationAsync(notification);
            }

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Notification marked as read successfully"));
        }

        public async Task<ApiResponse<PagedResult<UserDeviceResponse>>> GetAllUserDevicesAysnc(PaginationRequest paginationRequest)
        {
            var userDevices = await _repository.GetAllUserDevicesAsync();

            if (userDevices == null || userDevices.Count() == 0)
                return new ApiResponse<PagedResult<UserDeviceResponse>>(StatusCodes.Status404NotFound, "No user devices found");

            var pagedUserDevices = await userDevices.ToPagedResultAsync(paginationRequest);
            var response = _userDeviceMapper.ToUserDeviceResponseList(pagedUserDevices.Items);
            var result = new PagedResult<UserDeviceResponse>(response, pagedUserDevices.PaginationRequest, pagedUserDevices.TotalCount);

            return new ApiResponse<PagedResult<UserDeviceResponse>>(StatusCodes.Status200OK, result);
        }

        public async Task<ApiResponse<PagedResult<UserDeviceResponse>>> GetAllUserDevicesByUserIdAsync(Guid userId, PaginationRequest paginationRequest)
        {
            if (userId == Guid.Empty)
                return new ApiResponse<PagedResult<UserDeviceResponse>>(StatusCodes.Status400BadRequest, "Invalid user id");

            var userDevices = await _repository.GetAllUserDevicesByUserIdAsync(userId);

            if (userDevices == null || userDevices.Count() == 0)
                return new ApiResponse<PagedResult<UserDeviceResponse>>(StatusCodes.Status404NotFound, "No user devices found");

            var pagedUserDevices = await userDevices.ToPagedResultAsync(paginationRequest);
            var response = _userDeviceMapper.ToUserDeviceResponseList(pagedUserDevices.Items);
            var result = new PagedResult<UserDeviceResponse>(response, pagedUserDevices.PaginationRequest, pagedUserDevices.TotalCount);

            return new ApiResponse<PagedResult<UserDeviceResponse>>(StatusCodes.Status200OK, result);
        }

        public async Task<ApiResponse<UserDeviceResponse>> RegisterDeviceAsync(Guid userId, RegisterDeviceRequest request)
        {
            if (userId == Guid.Empty)
                return new ApiResponse<UserDeviceResponse>(StatusCodes.Status400BadRequest, "Invalid user id");

            if (string.IsNullOrWhiteSpace(request.DeviceToken))
                return new ApiResponse<UserDeviceResponse>(StatusCodes.Status400BadRequest, "Device token is required");

            var deviceToken = request.DeviceToken.Trim();
            var existingDevice = await _repository.GetUserDeviceByDeviceTokenAsync(deviceToken);

            if (existingDevice == null)
            {
                existingDevice = new UserDevice
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    DeviceToken = deviceToken,
                    DeviceType = request.DeviceType,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _repository.CreateUserDeviceAsync(existingDevice);
            }
            else
            {
                existingDevice.UserId = userId;
                existingDevice.DeviceType = request.DeviceType;
                existingDevice.IsActive = true;
                existingDevice.DeletedAt = null;
                existingDevice.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateUserDeviceAsync(existingDevice);
            }

            var response = _userDeviceMapper.ToUserDeviceResponse(existingDevice);
            return new ApiResponse<UserDeviceResponse>(StatusCodes.Status200OK, response);
        }

        public async Task<ApiResponse<ConfirmationResponse>> UnregisterDeviceAsync(Guid userId, UnregisterDeviceRequest request)
        {
            if (userId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid user id");

            if (string.IsNullOrWhiteSpace(request.DeviceToken))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Device token is required");

            var device = await _repository.GetUserDeviceByDeviceTokenAsync(request.DeviceToken.Trim());
            if (device == null || device.DeletedAt != null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No user device found");

            if (device.UserId != userId)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status403Forbidden, "You can only unregister your own device");

            device.IsActive = false;
            device.DeletedAt = DateTime.UtcNow;
            device.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateUserDeviceAsync(device);

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Unregister device successfully"));
        }
    }
}
