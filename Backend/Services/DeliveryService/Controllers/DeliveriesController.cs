using DeliveryService.DTOs;
using DeliveryService.Entities;
using DeliveryService.Services.Interfaces;
using Messaging.Contracts.Common;
using Messaging.Contracts.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<ApiResponse<PagedResult<ShipperAvailability>>>> GetAllAvailabilities([FromQuery] PaginationRequest paginationRequest)
        {
            var query = await _deliveryService.GetAllShipperAvailabilitiesAsync();

            if (query == null || !query.Any())
                return NotFound(new ApiResponse<PagedResult<ShipperAvailability>>(StatusCodes.Status404NotFound, "No shipper availability found"));

            var paged = await query.ToPagedResultAsync(paginationRequest);
            return Ok(new ApiResponse<PagedResult<ShipperAvailability>>(StatusCodes.Status200OK, paged));
        }

        [HttpGet("shippers/{shipperId:guid}/availability")]
        public async Task<ActionResult<ApiResponse<ShipperAvailability>>> GetAvailabilityByShipperId([FromRoute] Guid shipperId)
        {
            if (shipperId == Guid.Empty)
                return BadRequest(new ApiResponse<ShipperAvailability>(StatusCodes.Status400BadRequest, "Invalid shipper id"));

            var availability = await _deliveryService.GetShipperAvailabilityAsync(shipperId);
            if (availability == null)
                return NotFound(new ApiResponse<ShipperAvailability>(StatusCodes.Status404NotFound, "No shipper availability found"));

            return Ok(new ApiResponse<ShipperAvailability>(StatusCodes.Status200OK, availability));
        }

        [HttpPost("availability/toggle")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> ToggleAvailability([FromQuery] Guid shipperId, [FromBody] ToggleAvailabilityRequest request)
        {
            if (shipperId == Guid.Empty)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid shipper id"));

            var success = await _deliveryService.ToggleShipperAvailabilityAsync(shipperId, request);
            if (!success)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Failed to update availability"));

            return Ok(new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Update shipper availability successfully")));
        }

        [HttpPost("locations")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateLocation([FromBody] UpdateLocationRequest request)
        {
            if (request.OrderId == Guid.Empty || request.ShipperId == Guid.Empty)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid location payload"));

            var success = await _deliveryService.UpdateLocationAsync(request);
            if (!success)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Failed to update location"));

            return Ok(new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Update location successfully")));
        }

        [HttpGet("assignments")]
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

            return Ok(new ApiResponse<ShipperAssignment>(StatusCodes.Status200OK, assignment));
        }

        [HttpGet("shippers/{shipperId:guid}/assignments")]
        public async Task<ActionResult<ApiResponse<PagedResult<ShipperAssignment>>>> GetAssignmentsByShipperId([FromRoute] Guid shipperId, [FromQuery] PaginationRequest paginationRequest)
        {
            if (shipperId == Guid.Empty)
                return BadRequest(new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status400BadRequest, "Invalid shipper id"));

            var query = await _deliveryService.GetAssignmentsByShipperIdAsync(shipperId);
            if (query == null || !query.Any())
                return NotFound(new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status404NotFound, "No shipper assignments found"));

            var paged = await query.ToPagedResultAsync(paginationRequest);
            return Ok(new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status200OK, paged));
        }

        [HttpPost("assignments/accept")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> AcceptAssignment([FromBody] AcceptAssignmentRequest request)
        {
            if (request.AssignmentId == Guid.Empty)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid assignment id"));

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
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateAssignmentStatus([FromRoute] Guid assignmentId, [FromBody] UpdateDeliveryStatusRequest request)
        {
            if (assignmentId == Guid.Empty)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid assignment id"));

            var (success, message) = await _deliveryService.UpdateAssignmentStatusAsync(assignmentId, request);

            if (!success)
                return BadRequest(new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, message));

            return Ok(new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse(message)));
        }

        [HttpGet("files/upload-url")]
        public ActionResult<ApiResponse<PresignUrlResponse>> GetUploadUrl([FromQuery] Guid orderId, [FromQuery] Guid shipperId, [FromQuery] string stage, [FromQuery] string fileName, [FromQuery] string contentType)
        {
            if (orderId == Guid.Empty || shipperId == Guid.Empty || string.IsNullOrWhiteSpace(stage) || string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(contentType))
                return BadRequest(new ApiResponse<PresignUrlResponse>(StatusCodes.Status400BadRequest, "Invalid upload url request"));

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
    }
}
