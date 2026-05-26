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

        [HttpPatch("shippers/{shipperId:guid}/location")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateShipperLocation([FromRoute] Guid shipperId, [FromBody] UpdateShipperLocationRequest request)
        {
            try
            {
                var response = await _deliveryService.UpdateShipperLocationAsync(shipperId, request, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("orders/{orderId:guid}/location-history")]
        public async Task<ActionResult<ApiResponse<PagedResult<ShipperLocationHistory>>>> GetLocationHistoryByOrderId([FromRoute] Guid orderId, [FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                var response = await _deliveryService.GetLocationHistoryByOrderIdAsync(orderId, paginationRequest, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("shippers/{shipperId:guid}/location-history")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<PagedResult<ShipperLocationHistory>>>> GetLocationHistoryByShipperId([FromRoute] Guid shipperId, [FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                var response = await _deliveryService.GetLocationHistoryByShipperIdAsync(shipperId, paginationRequest, User);

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

        [HttpPost("incidents")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> ReportIncident([FromBody] ReportIncidentRequest request)
        {
            try
            {
                var response = await _deliveryService.ReportIncidentAsync(request, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("incidents")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<PagedResult<Incident>>>> GetAllIncidents([FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                var response = await _deliveryService.GetAllIncidentsAsync(paginationRequest);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("users/{reporterId:guid}/incidents")]
        public async Task<ActionResult<ApiResponse<PagedResult<Incident>>>> GetIncidentsByReporterId([FromRoute] Guid reporterId, [FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                var response = await _deliveryService.GetIncidentsByReporterIdAsync(reporterId, paginationRequest, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("incidents/{incidentId:guid}")]
        public async Task<ActionResult<ApiResponse<Incident>>> GetIncidentById([FromRoute] Guid incidentId)
        {
            try
            {
                var response = await _deliveryService.GetIncidentByIdAsync(incidentId, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("incidents/{incidentId:guid}/resolve")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> ResolveIncident([FromRoute] Guid incidentId, [FromBody] ResolveIncidentRequest request)
        {
            try
            {
                var response = await _deliveryService.ResolveIncidentAsync(incidentId, request, User);

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
