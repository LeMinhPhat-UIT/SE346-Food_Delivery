using Messaging.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Services.Interfaces;
using System.Security.Claims;

namespace NotificationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("devices")]
        public async Task<ActionResult<ApiResponse<UserDeviceResponse>>> RegisterDevice([FromBody] RegisterDeviceRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user context");

                var response = await _notificationService.RegisterDeviceAsync(userId.Value, request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("devices")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UnregisterDevice([FromBody] UnregisterDeviceRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user context");

                var response = await _notificationService.UnregisterDeviceAsync(userId.Value, request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("devices")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<PagedResult<UserDeviceResponse>>>> GetAllUserDevices([FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                var response = await _notificationService.GetAllUserDevicesAysnc(paginationRequest);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("users/{userId:guid}/devices")]
        public async Task<ActionResult<ApiResponse<PagedResult<UserDeviceResponse>>>> GetUserDevices(Guid userId, [FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    var currentUserId = GetCurrentUserId();
                    if (!currentUserId.HasValue)
                        return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user context");

                    if (currentUserId.Value != userId)
                        return StatusCode(StatusCodes.Status403Forbidden, "You can only view your own devices");
                }

                var response = await _notificationService.GetAllUserDevicesByUserIdAsync(userId, paginationRequest);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("userId");

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private bool IsCurrentUserAdmin()
        {
            var roles = User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Concat(User.FindAll("role").Select(c => c.Value));

            return roles.Any(role => string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase));
        }
    }
}
