using DeliveryService.DTOs;
using DeliveryService.Entities;
using DeliveryService.Enums;
using DeliveryService.Helpers;
using DeliveryService.Repositories.Interfaces;
using DeliveryService.Services.Interfaces;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Publishing;

namespace DeliveryService.Services.Implements
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly FirebaseStorageHelper _storageHelper;

        public DeliveryService(
            IDeliveryRepository deliveryRepository,
            IEventPublisher eventPublisher,
            FirebaseStorageHelper storageHelper)
        {
            _deliveryRepository = deliveryRepository;
            _eventPublisher = eventPublisher;
            _storageHelper = storageHelper;
        }

        public async Task<IQueryable<ShipperAvailability>?> GetAllShipperAvailabilitiesAsync()
        {
            return await _deliveryRepository.GetAllShipperAvailabilityAsync();
        }

        public async Task<ShipperAvailability?> GetShipperAvailabilityAsync(Guid shipperId)
        {
            return await _deliveryRepository.GetShipperAvailabilityByShipperIdAsync(shipperId);
        }

        public async Task<bool> ToggleShipperAvailabilityAsync(Guid shipperId, ToggleAvailabilityRequest request)
        {
            var existingAvailability = await _deliveryRepository.GetShipperAvailabilityByShipperIdAsync(shipperId);
            var availability = existingAvailability ?? new ShipperAvailability
            {
                Id = Guid.NewGuid(),
                ShipperId = shipperId
            };

            availability.Status = request.IsGoOnline ? ShipperWorkStatus.ActiveIdle : ShipperWorkStatus.Offline;
            availability.LastSeenAt = DateTime.UtcNow;

            if (request.IsGoOnline)
            {
                if (request.Lat.HasValue)
                    availability.CurrentLat = request.Lat.Value;

                if (request.Lng.HasValue)
                    availability.CurrentLng = request.Lng.Value;
            }
            else
            {
                availability.CurrentOrderId = null;
            }

            if (existingAvailability == null)
                await _deliveryRepository.CreateShipperAvailabilityAsync(availability);
            else
                await _deliveryRepository.UpdateShipperAvailabilityAsync(availability);

            return true;
        }

        public async Task<bool> UpdateLocationAsync(UpdateLocationRequest request)
        {
            var existingAvailability = await _deliveryRepository.GetShipperAvailabilityByShipperIdAsync(request.ShipperId);
            var availability = existingAvailability ?? new ShipperAvailability
            {
                Id = Guid.NewGuid(),
                ShipperId = request.ShipperId
            };

            availability.CurrentOrderId = request.OrderId;
            availability.CurrentLat = request.Latitude;
            availability.CurrentLng = request.Longitude;
            availability.LastSeenAt = DateTime.UtcNow;

            if (existingAvailability == null)
                await _deliveryRepository.CreateShipperAvailabilityAsync(availability);
            else
                await _deliveryRepository.UpdateShipperAvailabilityAsync(availability);

            return true;
        }

        public async Task<IQueryable<ShipperAssignment>?> GetAllAssignmentsAsync()
        {
            return await _deliveryRepository.GetAllShipperAssignmentsAsync();
        }

        public async Task<ShipperAssignment?> GetAssignmentByIdAsync(Guid assignmentId)
        {
            return await _deliveryRepository.GetShipperAssignmentByIdAsync(assignmentId);
        }

        public async Task<IQueryable<ShipperAssignment>?> GetAssignmentsByShipperIdAsync(Guid shipperId)
        {
            return await _deliveryRepository.GetAllShipperAssignmentsByShipperIdAsync(shipperId);
        }

        public async Task<(bool Success, string Message)> AcceptOrRejectAssignmentAsync(Guid assignmentId, AcceptAssignmentRequest request)
        {
            var assignment = await _deliveryRepository.GetShipperAssignmentByIdAsync(assignmentId);
            if (assignment == null)
                return (false, "No shipper assignment found");

            if (assignment.Status != AssignmentStatus.Pending)
                return (false, "Assignment has already been handled");

            if (request.IsAccepted)
            {
                // Rate condition: Check if another shipper has already accepted this order
                var acceptedAssignment = await _deliveryRepository.GetAcceptedShipperAssignmentByOrderIdAsync(assignment.OrderId);
                if (acceptedAssignment != null && acceptedAssignment.Id != assignment.Id)
                    return (false, "This order has already been accepted by another shipper");

                // Update the accepted assignment
                assignment.Status = AssignmentStatus.Accepted;
                assignment.AcceptedAt = DateTime.UtcNow;
                assignment.RespondedAt = DateTime.UtcNow;
                assignment.RejectReason = null;

                await _deliveryRepository.UpdateShipperAssignment(assignment);

                // Update shipper availability to PendingAssignment
                var existingAvailability = await _deliveryRepository.GetShipperAvailabilityByShipperIdAsync(assignment.ShipperId);
                var availability = existingAvailability ?? new ShipperAvailability
                {
                    Id = Guid.NewGuid(),
                    ShipperId = assignment.ShipperId
                };

                availability.CurrentOrderId = assignment.OrderId;
                availability.Status = ShipperWorkStatus.PendingAssignment;
                availability.LastSeenAt = DateTime.UtcNow;

                if (existingAvailability == null)
                    await _deliveryRepository.CreateShipperAvailabilityAsync(availability);
                else
                    await _deliveryRepository.UpdateShipperAvailabilityAsync(availability);

                // Cancel all other pending assignments for the same order
                var otherAssignments = await _deliveryRepository.GetAllShipperAssignmentsByOrderIdAsync(assignment.OrderId);
                foreach (var otherAssignment in otherAssignments.Where(item => item != null && item.Id != assignment.Id && item.Status == AssignmentStatus.Pending))
                {
                    otherAssignment!.Status = AssignmentStatus.Cancelled;
                    otherAssignment.RespondedAt = DateTime.UtcNow;
                    await _deliveryRepository.UpdateShipperAssignment(otherAssignment);
                }

                return (true, "Accept assignment successfully");
            }

            // Handle rejection
            if (string.IsNullOrWhiteSpace(request.RejectReason))
                return (false, "Reject reason is required when rejecting an assignment");

            assignment.Status = AssignmentStatus.Rejected;
            assignment.RespondedAt = DateTime.UtcNow;
            assignment.RejectReason = request.RejectReason.Trim();

            await _deliveryRepository.UpdateShipperAssignment(assignment);

            return (true, "Reject assignment successfully");
        }

        public async Task<(bool Success, string Message)> UpdateAssignmentStatusAsync(Guid assignmentId, UpdateDeliveryStatusRequest request)
        {
            var assignment = await _deliveryRepository.GetShipperAssignmentByIdAsync(assignmentId);
            if (assignment == null)
                return (false, "No shipper assignment found");

            if (request.Status == DeliveryStatus.PickedUp)
            {
                // Validate pickup milestone transition
                if (assignment.Status != AssignmentStatus.Accepted || assignment.PickedUpAt != null)
                    return (false, "Assignment is not ready for pickup confirmation");

                // Record pickup
                assignment.PickedUpAt = DateTime.UtcNow;
                assignment.PickupProofFileKey = request.ProofFileKey;

                // Update shipper availability to Delivering
                var existingAvailability = await _deliveryRepository.GetShipperAvailabilityByShipperIdAsync(assignment.ShipperId);
                var availability = existingAvailability ?? new ShipperAvailability
                {
                    Id = Guid.NewGuid(),
                    ShipperId = assignment.ShipperId
                };

                availability.CurrentOrderId = assignment.OrderId;
                availability.Status = ShipperWorkStatus.Delivering;
                availability.LastSeenAt = DateTime.UtcNow;

                if (existingAvailability == null)
                    await _deliveryRepository.CreateShipperAvailabilityAsync(availability);
                else
                    await _deliveryRepository.UpdateShipperAvailabilityAsync(availability);

                await _deliveryRepository.UpdateShipperAssignment(assignment);

                // Publish milestone event
                await _eventPublisher.PublishAsync(new DeliveryMilestoneEvent
                {
                    OrderId = assignment.OrderId,
                    OrderNumber = assignment.OrderNumber,
                    CustomerId = assignment.CustomerId,
                    ShipperId = assignment.ShipperId,
                    Milestone = DeliveryMilestoneType.PickedUp,
                    ProofFileKey = request.ProofFileKey,
                    Note = request.Note
                });

                return (true, "Confirm pickup successfully");
            }

            if (request.Status == DeliveryStatus.Delivered)
            {
                // Validate delivery milestone transition
                if (assignment.PickedUpAt == null || assignment.DeliveredAt != null)
                    return (false, "Assignment is not ready for delivery confirmation");

                // Record delivery
                assignment.DeliveredAt = DateTime.UtcNow;
                assignment.DeliveryProofFileKey = request.ProofFileKey;

                // Update shipper availability back to ActiveIdle
                var existingAvailability = await _deliveryRepository.GetShipperAvailabilityByShipperIdAsync(assignment.ShipperId);
                var availability = existingAvailability ?? new ShipperAvailability
                {
                    Id = Guid.NewGuid(),
                    ShipperId = assignment.ShipperId
                };

                availability.CurrentOrderId = null;
                availability.Status = ShipperWorkStatus.ActiveIdle;
                availability.LastSeenAt = DateTime.UtcNow;

                if (existingAvailability == null)
                    await _deliveryRepository.CreateShipperAvailabilityAsync(availability);
                else
                    await _deliveryRepository.UpdateShipperAvailabilityAsync(availability);

                await _deliveryRepository.UpdateShipperAssignment(assignment);

                // Publish milestone event
                await _eventPublisher.PublishAsync(new DeliveryMilestoneEvent
                {
                    OrderId = assignment.OrderId,
                    OrderNumber = assignment.OrderNumber,
                    CustomerId = assignment.CustomerId,
                    ShipperId = assignment.ShipperId,
                    Milestone = DeliveryMilestoneType.Delivered,
                    ProofFileKey = request.ProofFileKey,
                    Note = request.Note
                });

                return (true, "Confirm delivery successfully");
            }

            return (false, "Unsupported delivery status transition");
        }

        public string GetUploadUrl(Guid orderId, Guid shipperId, string stage, string fileName, string contentType)
        {
            var extension = Path.GetExtension(fileName);
            var newFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = $"deliveries/{orderId}/{shipperId}/{stage.Trim().ToLowerInvariant()}/{newFileName}";

            return _storageHelper.GenerateUploadUrl(filePath, contentType);
        }
    }
}
