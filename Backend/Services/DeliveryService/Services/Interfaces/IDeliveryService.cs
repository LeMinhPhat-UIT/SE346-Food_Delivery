using DeliveryService.DTOs;
using DeliveryService.Entities;
using Messaging.Contracts.Common;
using System.Security.Claims;

namespace DeliveryService.Services.Interfaces
{
    public interface IDeliveryService
    {
        Task<ApiResponse<PagedResult<ShipperAvailability>>> GetAllShipperAvailabilitiesAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<ShipperAvailability>> GetShipperAvailabilityAsync(Guid shipperId, ClaimsPrincipal user);
        Task<ApiResponse<ConfirmationResponse>> ToggleShipperAvailabilityAsync(Guid shipperId, ToggleAvailabilityRequest request, ClaimsPrincipal user);
        Task<ApiResponse<ConfirmationResponse>> UpdateLocationAsync(UpdateLocationRequest request, ClaimsPrincipal user);
        Task<ApiResponse<ConfirmationResponse>> UpdateShipperLocationAsync(Guid shipperId, UpdateShipperLocationRequest request, ClaimsPrincipal user);
        Task<ApiResponse<PagedResult<ShipperLocationHistory>>> GetLocationHistoryByOrderIdAsync(Guid orderId, PaginationRequest paginationRequest, ClaimsPrincipal user);
        Task<ApiResponse<PagedResult<ShipperLocationHistory>>> GetLocationHistoryByShipperIdAsync(Guid shipperId, PaginationRequest paginationRequest, ClaimsPrincipal user);
        Task<ApiResponse<PagedResult<ShipperAssignment>>> GetAllAssignmentsAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<ShipperAssignment>> GetAssignmentByIdAsync(Guid assignmentId, ClaimsPrincipal user);
        Task<ApiResponse<PagedResult<ShipperAssignment>>> GetAssignmentsByShipperIdAsync(Guid shipperId, PaginationRequest paginationRequest, ClaimsPrincipal user);
        Task<ApiResponse<ConfirmationResponse>> AcceptOrRejectAssignmentAsync(AcceptAssignmentRequest request, ClaimsPrincipal user);
        Task<ApiResponse<ConfirmationResponse>> UpdateAssignmentStatusAsync(Guid assignmentId, UpdateDeliveryStatusRequest request, ClaimsPrincipal user);
        Task<ApiResponse<EstimateDeliveryFeeResponse>> EstimateDeliveryFeeAsync(EstimateDeliveryFeeRequest? request);
        Task<ApiResponse<ConfirmationResponse>> ReportIncidentAsync(ReportIncidentRequest request, ClaimsPrincipal user);
        Task<ApiResponse<PagedResult<Incident>>> GetAllIncidentsAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<PagedResult<Incident>>> GetIncidentsByReporterIdAsync(Guid reporterId, PaginationRequest paginationRequest, ClaimsPrincipal user);
        Task<ApiResponse<Incident>> GetIncidentByIdAsync(Guid incidentId, ClaimsPrincipal user);
        Task<ApiResponse<ConfirmationResponse>> ResolveIncidentAsync(Guid incidentId, ResolveIncidentRequest request, ClaimsPrincipal user);
    }
}
