using DeliveryService.DTOs;
using DeliveryService.Entities;
using DeliveryService.Enums;
using DeliveryService.Services.Interfaces;
using Messaging.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

        [HttpPost("assignments/{assignmentId:guid}/accept")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> AcceptAssignmentOffer([FromRoute] Guid assignmentId)
        {
            try
            {
                var response = await _deliveryService.AcceptAssignmentAsync(assignmentId, null, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("assignments/{assignmentId:guid}/reject")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> RejectAssignmentOffer([FromRoute] Guid assignmentId, [FromBody] RejectAssignmentRequest request)
        {
            try
            {
                var response = await _deliveryService.RejectAssignmentAsync(assignmentId, request, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("shippers/me/active-offer")]
        [Authorize(Policy = "ShipperOrAdmin")]
        public async Task<ActionResult<ApiResponse<ActiveAssignmentOfferResponse>>> GetActiveOffer()
        {
            try
            {
                var response = await _deliveryService.GetActiveOfferAsync(User);

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
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateAssignmentStatus([FromRoute] Guid assignmentId)
        {
            try
            {
                var parsedRequest = await ReadUpdateDeliveryStatusRequestAsync();
                if (parsedRequest.Error != null)
                {
                    var errorResponse = new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, parsedRequest.Error);
                    return StatusCode(errorResponse.StatusCode, errorResponse);
                }

                var request = parsedRequest.Request!;
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

        private async Task<(UpdateDeliveryStatusRequest? Request, string? Error)> ReadUpdateDeliveryStatusRequestAsync()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
                return (null, "Delivery status request body is required");

            var trimmedBody = body.Trim();

            if (trimmedBody.StartsWith("{"))
                return ParseUpdateDeliveryStatusObject(trimmedBody);

            if (trimmedBody.StartsWith("\""))
            {
                try
                {
                    var statusText = JsonSerializer.Deserialize<string>(trimmedBody);
                    return ParseUpdateDeliveryStatusText(statusText);
                }
                catch (JsonException)
                {
                    return (null, "Invalid delivery status request body");
                }
            }

            return ParseUpdateDeliveryStatusText(trimmedBody);
        }

        private static (UpdateDeliveryStatusRequest? Request, string? Error) ParseUpdateDeliveryStatusObject(string body)
        {
            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                    return (null, "Delivery status request body must be an object or status string");

                if (!TryGetProperty(root, nameof(UpdateDeliveryStatusRequest.Status), out var statusElement))
                    return (null, "Status is required");

                if (!TryReadDeliveryStatus(statusElement, out var status))
                    return (null, "Invalid delivery status");

                return (new UpdateDeliveryStatusRequest
                {
                    Status = status,
                    Note = GetOptionalString(root, nameof(UpdateDeliveryStatusRequest.Note)) ?? string.Empty,
                    ProofFileKey = GetOptionalString(root, nameof(UpdateDeliveryStatusRequest.ProofFileKey))
                }, null);
            }
            catch (JsonException)
            {
                return (null, "Invalid delivery status request body");
            }
        }

        private static (UpdateDeliveryStatusRequest? Request, string? Error) ParseUpdateDeliveryStatusText(string? statusText)
        {
            if (!TryParseDeliveryStatus(statusText, out var status))
                return (null, "Invalid delivery status");

            return (new UpdateDeliveryStatusRequest
            {
                Status = status,
                Note = string.Empty
            }, null);
        }

        private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement property)
        {
            foreach (var item in element.EnumerateObject())
            {
                if (string.Equals(item.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    property = item.Value;
                    return true;
                }
            }

            property = default;
            return false;
        }

        private static string? GetOptionalString(JsonElement element, string propertyName)
        {
            if (!TryGetProperty(element, propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
                return null;

            return property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : property.ToString();
        }

        private static bool TryReadDeliveryStatus(JsonElement element, out DeliveryStatus status)
        {
            if (element.ValueKind == JsonValueKind.String)
                return TryParseDeliveryStatus(element.GetString(), out status);

            if (element.ValueKind == JsonValueKind.Number &&
                element.TryGetInt32(out var value) &&
                Enum.IsDefined(typeof(DeliveryStatus), value))
            {
                status = (DeliveryStatus)value;
                return true;
            }

            status = default;
            return false;
        }

        private static bool TryParseDeliveryStatus(string? value, out DeliveryStatus status)
        {
            var normalized = value?
                .Replace("\uFEFF", string.Empty)
                .Replace("\u200B", string.Empty)
                .Trim();

            return Enum.TryParse(normalized, ignoreCase: true, out status) &&
                   Enum.IsDefined(typeof(DeliveryStatus), status);
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
