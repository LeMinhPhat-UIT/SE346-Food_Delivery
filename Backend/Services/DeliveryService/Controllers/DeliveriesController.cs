using DeliveryService.DTOs;
using DeliveryService.Entities;
using DeliveryService.Services.Interfaces;
using Messaging.Contracts.Common;
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

        [HttpPost("estimate-fee")]
        public async Task<ActionResult<ApiResponse<EstimateDeliveryFeeResponse>>> EstimateDeliveryFee([FromBody] EstimateDeliveryFeeRequest? request)
        {
            try
            {
                var response = await _deliveryService.EstimateDeliveryFeeAsync(request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("availabilities")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<PagedResult<ShipperAvailability>>>> GetAllAvailabilities([FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                var response = await _deliveryService.GetAllShipperAvailabilitiesAsync(paginationRequest);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("shippers/{shipperId:guid}/availability")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ShipperAvailability>>> GetAvailabilityByShipperId([FromRoute] Guid shipperId)
        {
            try
            {
                var response = await _deliveryService.GetShipperAvailabilityAsync(shipperId, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("availability/toggle")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> ToggleAvailability([FromQuery] Guid shipperId, [FromBody] ToggleAvailabilityRequest request)
        {
            try
            {
                var response = await _deliveryService.ToggleShipperAvailabilityAsync(shipperId, request, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("locations")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateLocation([FromBody] UpdateLocationRequest request)
        {
            try
            {
                var response = await _deliveryService.UpdateLocationAsync(request, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("assignments")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<PagedResult<ShipperAssignment>>>> GetAllAssignments([FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                var response = await _deliveryService.GetAllAssignmentsAsync(paginationRequest);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("assignments/{assignmentId:guid}")]
        public async Task<ActionResult<ApiResponse<ShipperAssignment>>> GetAssignmentById([FromRoute] Guid assignmentId)
        {
            try
            {
                var response = await _deliveryService.GetAssignmentByIdAsync(assignmentId, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("shippers/{shipperId:guid}/assignments")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<PagedResult<ShipperAssignment>>>> GetAssignmentsByShipperId([FromRoute] Guid shipperId, [FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                var response = await _deliveryService.GetAssignmentsByShipperIdAsync(shipperId, paginationRequest, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("assignments/accept")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> AcceptAssignment([FromBody] AcceptAssignmentRequest request)
        {
            try
            {
                var response = await _deliveryService.AcceptOrRejectAssignmentAsync(request, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("assignments/{assignmentId:guid}/status")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateAssignmentStatus([FromRoute] Guid assignmentId, [FromBody] UpdateDeliveryStatusRequest request)
        {
            try
            {
                var response = await _deliveryService.UpdateAssignmentStatusAsync(assignmentId, request, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("files/upload-url")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public ActionResult<ApiResponse<PresignUrlResponse>> GetUploadUrl([FromQuery] Guid orderId, [FromQuery] Guid shipperId, [FromQuery] string stage, [FromQuery] string fileName, [FromQuery] string contentType)
        {
            try
            {
                var response = _deliveryService.GetUploadUrl(orderId, shipperId, stage, fileName, contentType, User);

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
