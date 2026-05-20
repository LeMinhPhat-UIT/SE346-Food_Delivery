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
        Task<ApiResponse<PagedResult<ShipperAssignment>>> GetAllAssignmentsAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<ShipperAssignment>> GetAssignmentByIdAsync(Guid assignmentId, ClaimsPrincipal user);
        Task<ApiResponse<PagedResult<ShipperAssignment>>> GetAssignmentsByShipperIdAsync(Guid shipperId, PaginationRequest paginationRequest, ClaimsPrincipal user);
        Task<ApiResponse<ConfirmationResponse>> AcceptOrRejectAssignmentAsync(AcceptAssignmentRequest request, ClaimsPrincipal user);
        Task<ApiResponse<ConfirmationResponse>> UpdateAssignmentStatusAsync(Guid assignmentId, UpdateDeliveryStatusRequest request, ClaimsPrincipal user);
        Task<ApiResponse<EstimateDeliveryFeeResponse>> EstimateDeliveryFeeAsync(EstimateDeliveryFeeRequest? request);
        ApiResponse<PresignUrlResponse> GetUploadUrl(Guid orderId, Guid shipperId, string stage, string fileName, string contentType, ClaimsPrincipal user);
    }
}
