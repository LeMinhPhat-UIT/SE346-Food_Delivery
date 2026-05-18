using DeliveryService.DTOs;
using DeliveryService.Entities;
using DeliveryService.Services.Interfaces;
using Messaging.Contracts.Common;
using Messaging.Contracts.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeliveryService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeliveriesController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;

        public DeliveriesController(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }

        [HttpGet("availabilities")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<PagedResult<ShipperAvailability>>>> GetAllAvailabilities([FromQuery] PaginationRequest paginationRequest)
        {
            var query = await _deliveryService.GetAllShipperAvailabilitiesAsync();

            if (query == null || !query.Any())
                return NotFound(new ApiResponse<PagedResult<ShipperAvailability>>(StatusCodes.Status404NotFound, "No shipper availability found"));

            var paged = await query.ToPagedResultAsync(paginationRequest);
            return Ok(new ApiResponse<PagedResult<ShipperAvailability>>(StatusCodes.Status200OK, paged));
        }

        [HttpGet("shippers/{shipperId:guid}/availability")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ShipperAvailability>>> GetAvailabilityByShipperId([FromRoute] Guid shipperId)
        {
            if (shipperId == Guid.Empty)
                return BadRequest(new ApiResponse<ShipperAvailability>(StatusCodes.Status400BadRequest, "Invalid shipper id"));

            if (!CanAccessShipper(shipperId))
                return StatusCode(StatusCodes.Status403Forbidden, "You can only access your own shipper availability");

            var availability = await _deliveryService.GetShipperAvailabilityAsync(shipperId);
            if (availability == null)
                return NotFound(new ApiResponse<ShipperAvailability>(StatusCodes.Status404NotFound, "No shipper availability found"));

            return Ok(new ApiResponse<ShipperAvailability>(StatusCodes.Status200OK, availability));
        }

        [HttpPost("availability/toggle")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> ToggleAvailability([FromQuery] Guid shipperId, [FromBody] ToggleAvailabilityRequest request)
        {
            if (shipperId == Guid.Empty)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid shipper id"));

            if (!CanAccessShipper(shipperId))
                return StatusCode(StatusCodes.Status403Forbidden, "You can only update your own shipper availability");

            var success = await _deliveryService.ToggleShipperAvailabilityAsync(shipperId, request);
            if (!success)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Failed to update availability"));

            return Ok(new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Update shipper availability successfully")));
        }

        [HttpPost("locations")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateLocation([FromBody] UpdateLocationRequest request)
        {
            if (request.OrderId == Guid.Empty || request.ShipperId == Guid.Empty)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid location payload"));

            if (!CanAccessShipper(request.ShipperId))
                return StatusCode(StatusCodes.Status403Forbidden, "You can only update your own shipper location");

            var success = await _deliveryService.UpdateLocationAsync(request);
            if (!success)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Failed to update location"));

            return Ok(new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Update location successfully")));
        }

        [HttpGet("assignments")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<PagedResult<ShipperAssignment>>>> GetAllAssignments([FromQuery] PaginationRequest paginationRequest)
        {
            var query = await _deliveryService.GetAllAssignmentsAsync();

            if (query == null || !query.Any())
                return NotFound(new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status404NotFound, "No shipper assignments found"));

            var paged = await query.ToPagedResultAsync(paginationRequest);
            return Ok(new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status200OK, paged));
        }

        [HttpGet("assignments/{assignmentId:guid}")]
        public async Task<ActionResult<ApiResponse<ShipperAssignment>>> GetAssignmentById([FromRoute] Guid assignmentId)
        {
            if (assignmentId == Guid.Empty)
                return BadRequest(new ApiResponse<ShipperAssignment>(StatusCodes.Status400BadRequest, "Invalid assignment id"));

            var assignment = await _deliveryService.GetAssignmentByIdAsync(assignmentId);
            if (assignment == null)
                return NotFound(new ApiResponse<ShipperAssignment>(StatusCodes.Status404NotFound, "No shipper assignment found"));

            if (!CanAccessAssignment(assignment))
                return StatusCode(StatusCodes.Status403Forbidden, "You can only access your own assignment");

            return Ok(new ApiResponse<ShipperAssignment>(StatusCodes.Status200OK, assignment));
        }

        [HttpGet("shippers/{shipperId:guid}/assignments")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<PagedResult<ShipperAssignment>>>> GetAssignmentsByShipperId([FromRoute] Guid shipperId, [FromQuery] PaginationRequest paginationRequest)
        {
            if (shipperId == Guid.Empty)
                return BadRequest(new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status400BadRequest, "Invalid shipper id"));

            if (!CanAccessShipper(shipperId))
                return StatusCode(StatusCodes.Status403Forbidden, "You can only access your own assignments");

            var query = await _deliveryService.GetAssignmentsByShipperIdAsync(shipperId);
            if (query == null || !query.Any())
                return NotFound(new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status404NotFound, "No shipper assignments found"));

            var paged = await query.ToPagedResultAsync(paginationRequest);
            return Ok(new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status200OK, paged));
        }

        [HttpPost("assignments/accept")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> AcceptAssignment([FromBody] AcceptAssignmentRequest request)
        {
            if (request.AssignmentId == Guid.Empty)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid assignment id"));

            var assignment = await _deliveryService.GetAssignmentByIdAsync(request.AssignmentId);
            if (assignment == null)
                return NotFound(new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No shipper assignment found"));

            if (!CanAccessShipper(assignment.ShipperId))
                return StatusCode(StatusCodes.Status403Forbidden, "You can only respond to your own assignment");

            var (success, message) = await _deliveryService.AcceptOrRejectAssignmentAsync(request.AssignmentId, request);

            if (!success)
            {
                if (message.Contains("already accepted"))
                    return Conflict(new ApiResponse<ConfirmationResponse>(StatusCodes.Status409Conflict, message));

                if (message.Contains("handled") || message.Contains("required") || message.Contains("ready") || message.Contains("unsupported", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, message));

                return NotFound(new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, message));
            }

            return Ok(new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse(message)));
        }

        [HttpPost("assignments/{assignmentId:guid}/status")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateAssignmentStatus([FromRoute] Guid assignmentId, [FromBody] UpdateDeliveryStatusRequest request)
        {
            if (assignmentId == Guid.Empty)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid assignment id"));

            var assignment = await _deliveryService.GetAssignmentByIdAsync(assignmentId);
            if (assignment == null)
                return NotFound(new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No shipper assignment found"));

            if (!CanAccessShipper(assignment.ShipperId))
                return StatusCode(StatusCodes.Status403Forbidden, "You can only update your own assignment");

            var (success, message) = await _deliveryService.UpdateAssignmentStatusAsync(assignmentId, request);

            if (!success)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, message));

            return Ok(new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse(message)));
        }

        [HttpGet("files/upload-url")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public ActionResult<ApiResponse<PresignUrlResponse>> GetUploadUrl([FromQuery] Guid orderId, [FromQuery] Guid shipperId, [FromQuery] string stage, [FromQuery] string fileName, [FromQuery] string contentType)
        {
            if (orderId == Guid.Empty || shipperId == Guid.Empty || string.IsNullOrWhiteSpace(stage) || string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(contentType))
                return BadRequest(new ApiResponse<PresignUrlResponse>(StatusCodes.Status400BadRequest, "Invalid upload url request"));

            if (!CanAccessShipper(shipperId))
                return StatusCode(StatusCodes.Status403Forbidden, "You can only create upload URLs for your own delivery");

            var (fileKey, uploadUrl) = _deliveryService.GetUploadUrl(orderId, shipperId, stage, fileName, contentType);

            return Ok(new ApiResponse<PresignUrlResponse>
            {
                Data = new PresignUrlResponse
                {
                    FileKey = fileKey,
                    UploadUrl = uploadUrl,
                    ContentType = contentType
                }
            });
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("userId");

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private Guid? GetCurrentShipperId()
        {
            var shipperIdClaim = User.FindFirstValue("shipperId")
                ?? User.FindFirstValue("ShipperId")
                ?? User.FindFirstValue("shipper_id");

            return Guid.TryParse(shipperIdClaim, out var shipperId) ? shipperId : null;
        }

        private bool IsCurrentUserInRole(params string[] allowedRoles)
        {
            var roles = User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Concat(User.FindAll("role").Select(c => c.Value));

            return roles.Any(role => allowedRoles.Any(allowed => string.Equals(role, allowed, StringComparison.OrdinalIgnoreCase)));
        }

        private bool IsCurrentUserAdmin()
        {
            return IsCurrentUserInRole("Admin", "ADMIN");
        }

        private bool IsCurrentUserShipper()
        {
            return IsCurrentUserInRole("Shipper", "SHIPPER");
        }

        private bool CanAccessShipper(Guid shipperId)
        {
            if (IsCurrentUserAdmin())
                return true;

            var currentShipperId = GetCurrentShipperId();
            if (currentShipperId.HasValue)
                return currentShipperId.Value == shipperId;

            return IsCurrentUserShipper();
        }

        private bool CanAccessAssignment(ShipperAssignment assignment)
        {
            if (IsCurrentUserAdmin())
                return true;

            var currentUserId = GetCurrentUserId();
            if (currentUserId.HasValue && currentUserId.Value == assignment.CustomerId)
                return true;

            return CanAccessShipper(assignment.ShipperId);
        }
    }
}
