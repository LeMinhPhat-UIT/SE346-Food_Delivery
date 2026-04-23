using Messaging.Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Enums;
using Microsoft.AspNetCore.Authorization;
using UserService.DTOs.MerchantDTOs;

namespace UserService.Controllers
{
    public partial class UsersController : ControllerBase
    {
        [HttpPost("merchant-request")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> RequestMerchantRole([FromBody] CreateMerchantRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user context");

                var response = await _userService.RequestForMerchantRole(userId.Value, request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("merchant-request")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<PagedResult<MerchantRequestResponse>>>> GetAllMerchantRequests([FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                //if (!IsCurrentUserAdmin())
                //    return StatusCode(StatusCodes.Status403Forbidden, "Only admin can view merchant requests");

                var response = await _userService.GetAllMerchantRequestsAsync(paginationRequest);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("merchant-request/{requestId:guid}/review")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> ReviewMerchantRequest(Guid requestId, [FromBody] ReviewMerchantRequest request)
        {
            try
            {
                //if (!IsCurrentUserAdmin())
                //    return StatusCode(StatusCodes.Status403Forbidden, "Only admin can review merchant requests");

                var reviewerId = GetCurrentUserId();
                if (!reviewerId.HasValue)
                    return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user context");

                var response = await _userService.ReviewMerchantRequestAsync(requestId, reviewerId.Value, request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("merchants")]
        public async Task<ActionResult<ApiResponse<PagedResult<MerchantResponse>>>> GetAllMerchants([FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                var response = await _userService.GetAllMerchantsAsync(paginationRequest);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("merchants/{merchantId:guid}")]
        public async Task<ActionResult<ApiResponse<MerchantResponse>>> GetMerchantById(Guid merchantId)
        {
            try
            {
                var response = await _userService.GetMerchantByIdAsync(merchantId);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("merchants/{merchantId:guid}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateMerchant(Guid merchantId, [FromBody] UpdateMerchantRequest request)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    var userId = GetCurrentUserId();
                    if (!userId.HasValue)
                        return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user context");

                    var merchantResult = await _userService.GetMerchantByIdAsync(merchantId);
                    if (!merchantResult.Success)
                        return StatusCode(merchantResult.StatusCode, merchantResult);

                    if (merchantResult.Data.UserId != userId.Value)
                        return StatusCode(StatusCodes.Status403Forbidden, "You can only update your own merchant profile");
                }

                var response = await _userService.UpdateMerchantAsync(merchantId, request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("merchants/{merchantId:guid}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> DeleteMerchant(Guid merchantId)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    var userId = GetCurrentUserId();
                    if (!userId.HasValue)
                        return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user context");

                    var merchantResult = await _userService.GetMerchantByIdAsync(merchantId);
                    if (!merchantResult.Success)
                        return StatusCode(merchantResult.StatusCode, merchantResult);

                    if (merchantResult.Data.UserId != userId.Value)
                        return StatusCode(StatusCodes.Status403Forbidden, "You can only delete your own merchant profile");
                }

                var response = await _userService.DeleteMerchantAsync(merchantId);

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

            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }

        private bool IsCurrentUserAdmin()
        {
            var roles = User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Concat(User.FindAll("role").Select(c => c.Value));

            return roles.Any(r => string.Equals(r, ApplicationRole.Admin, StringComparison.OrdinalIgnoreCase));
        }
    }
}
