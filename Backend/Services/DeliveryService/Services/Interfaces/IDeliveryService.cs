using DeliveryService.DTOs;
using DeliveryService.Entities;

namespace DeliveryService.Services.Interfaces
{
    public interface IDeliveryService
    {
        Task<IQueryable<ShipperAvailability>?> GetAllShipperAvailabilitiesAsync();
        Task<ShipperAvailability?> GetShipperAvailabilityAsync(Guid shipperId);
        Task<bool> ToggleShipperAvailabilityAsync(Guid shipperId, ToggleAvailabilityRequest request);
        Task<bool> UpdateLocationAsync(UpdateLocationRequest request);
        Task<IQueryable<ShipperAssignment>?> GetAllAssignmentsAsync();
        Task<ShipperAssignment?> GetAssignmentByIdAsync(Guid assignmentId);
        Task<IQueryable<ShipperAssignment>?> GetAssignmentsByShipperIdAsync(Guid shipperId);
        Task<(bool Success, string Message)> AcceptOrRejectAssignmentAsync(Guid assignmentId, AcceptAssignmentRequest request);
        Task<(bool Success, string Message)> UpdateAssignmentStatusAsync(Guid assignmentId, UpdateDeliveryStatusRequest request);
        (string FileKey, string UploadUrl) GetUploadUrl(Guid orderId, Guid shipperId, string stage, string fileName, string contentType);
    }
}
