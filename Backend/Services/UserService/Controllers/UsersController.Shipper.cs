using Messaging.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs.ShipperDTOs;

namespace UserService.Controllers
{
    public partial class UsersController : ControllerBase
    {
        [HttpPost("shipper-request")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> RequestShipperRole([FromBody] CreateShipperRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user context");

                var response = await _userService.RequestForShipperRole(userId.Value, request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("shipper-request")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<PagedResult<ShipperRequestResponse>>>> GetAllShipperRequests([FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                var response = await _userService.GetAllShipperRequestsAsync(paginationRequest);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("shipper-request/{requestId:guid}/review")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> ReviewShipperRequest(Guid requestId, [FromBody] ReviewShipperRequest request)
        {
            try
            {
                var reviewerId = GetCurrentUserId();
                if (!reviewerId.HasValue)
                    return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user context");

                var response = await _userService.ReviewShipperRequestAsync(requestId, reviewerId.Value, request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("shippers")]
        public async Task<ActionResult<ApiResponse<PagedResult<ShipperResponse>>>> GetAllShippers([FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                var response = await _userService.GetAllShippersAsync(paginationRequest);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("shippers/{shipperId:guid}")]
        public async Task<ActionResult<ApiResponse<ShipperResponse>>> GetShipperById(Guid shipperId)
        {
            try
            {
                var response = await _userService.GetShipperByIdAsync(shipperId);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("shippers/{shipperId:guid}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateShipper(Guid shipperId, [FromBody] UpdateShipperRequest request)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    var userId = GetCurrentUserId();
                    if (!userId.HasValue)
                        return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user context");

                    var shipperResult = await _userService.GetShipperByIdAsync(shipperId);
                    if (!shipperResult.Success)
                        return StatusCode(shipperResult.StatusCode, shipperResult);

                    if (shipperResult.Data!.UserId != userId.Value)
                        return StatusCode(StatusCodes.Status403Forbidden, "You can only update your own shipper profile");
                }

                var response = await _userService.UpdateShipperAsync(shipperId, request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("shippers/{shipperId:guid}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> DeleteShipper(Guid shipperId)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    var userId = GetCurrentUserId();
                    if (!userId.HasValue)
                        return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user context");

                    var shipperResult = await _userService.GetShipperByIdAsync(shipperId);
                    if (!shipperResult.Success)
                        return StatusCode(shipperResult.StatusCode, shipperResult);

                    if (shipperResult.Data!.UserId != userId.Value)
                        return StatusCode(StatusCodes.Status403Forbidden, "You can only delete your own shipper profile");
                }

                var response = await _userService.DeleteShipperAsync(shipperId);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
