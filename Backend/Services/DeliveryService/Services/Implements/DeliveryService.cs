using DeliveryService.DTOs;
using DeliveryService.Entities;
using DeliveryService.Enums;
using DeliveryService.Mappers;
using DeliveryService.Options;
using DeliveryService.Repositories.Interfaces;
using DeliveryService.Services.Interfaces;
using Messaging.Contracts.Common;
using Messaging.Contracts.Events;
using Messaging.Contracts.Extensions;
using Messaging.RabbitMq.Publishing;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace DeliveryService.Services.Implements
{
    public class DeliveryService : IDeliveryService
    {
        private const double EarthRadiusKm = 6371.0088d;
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IOptions<DeliveryOption> _deliveryOptions;
        private readonly DeliveryMapper _mapper;

        public DeliveryService(
            IDeliveryRepository deliveryRepository,
            IEventPublisher eventPublisher,
            IOptions<DeliveryOption> deliveryOptions,
            DeliveryMapper mapper)
        {
            _deliveryRepository = deliveryRepository;
            _eventPublisher = eventPublisher;
            _deliveryOptions = deliveryOptions;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedResult<ShipperAvailability>>> GetAllShipperAvailabilitiesAsync(PaginationRequest paginationRequest)
        {
            var query = await _deliveryRepository.GetAllShipperAvailabilityAsync();

            if (query == null || !query.Any())
                return new ApiResponse<PagedResult<ShipperAvailability>>(StatusCodes.Status404NotFound, "No shipper availability found");

            var paged = await query.ToPagedResultAsync(paginationRequest);
            return new ApiResponse<PagedResult<ShipperAvailability>>(StatusCodes.Status200OK, paged);
        }

        public async Task<ApiResponse<ShipperAvailability>> GetShipperAvailabilityAsync(Guid shipperId, ClaimsPrincipal user)
        {
            if (shipperId == Guid.Empty)
                return new ApiResponse<ShipperAvailability>(StatusCodes.Status400BadRequest, "Invalid shipper id");

            if (!CanAccessShipper(user, shipperId))
                return new ApiResponse<ShipperAvailability>(StatusCodes.Status403Forbidden, "You can only access your own shipper availability");

            var availability = await _deliveryRepository.GetShipperAvailabilityByShipperIdAsync(shipperId);
            if (availability == null)
                return new ApiResponse<ShipperAvailability>(StatusCodes.Status404NotFound, "No shipper availability found");

            return new ApiResponse<ShipperAvailability>(StatusCodes.Status200OK, availability);
        }

        public async Task<ApiResponse<ConfirmationResponse>> ToggleShipperAvailabilityAsync(Guid shipperId, ToggleAvailabilityRequest request, ClaimsPrincipal user)
        {
            if (shipperId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid shipper id");

            if (!CanAccessShipper(user, shipperId))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status403Forbidden, "You can only update your own shipper availability");

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

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Update shipper availability successfully"));
        }

        public async Task<ApiResponse<ConfirmationResponse>> UpdateLocationAsync(UpdateLocationRequest request, ClaimsPrincipal user)
        {
            if (request.OrderId == Guid.Empty || request.ShipperId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid location payload");

            if (!CanAccessShipper(user, request.ShipperId))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status403Forbidden, "You can only update your own shipper location");

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

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Update location successfully"));
        }

        public async Task<ApiResponse<PagedResult<ShipperAssignment>>> GetAllAssignmentsAsync(PaginationRequest paginationRequest)
        {
            var query = await _deliveryRepository.GetAllShipperAssignmentsAsync();

            if (query == null || !query.Any())
                return new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status404NotFound, "No shipper assignments found");

            var paged = await query.ToPagedResultAsync(paginationRequest);
            return new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status200OK, paged);
        }

        public async Task<ApiResponse<ShipperAssignment>> GetAssignmentByIdAsync(Guid assignmentId, ClaimsPrincipal user)
        {
            if (assignmentId == Guid.Empty)
                return new ApiResponse<ShipperAssignment>(StatusCodes.Status400BadRequest, "Invalid assignment id");

            var assignment = await _deliveryRepository.GetShipperAssignmentByIdAsync(assignmentId);
            if (assignment == null)
                return new ApiResponse<ShipperAssignment>(StatusCodes.Status404NotFound, "No shipper assignment found");

            if (!CanAccessAssignment(user, assignment))
                return new ApiResponse<ShipperAssignment>(StatusCodes.Status403Forbidden, "You can only access your own assignment");

            return new ApiResponse<ShipperAssignment>(StatusCodes.Status200OK, assignment);
        }

        public async Task<ApiResponse<PagedResult<ShipperAssignment>>> GetAssignmentsByShipperIdAsync(Guid shipperId, PaginationRequest paginationRequest, ClaimsPrincipal user)
        {
            if (shipperId == Guid.Empty)
                return new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status400BadRequest, "Invalid shipper id");

            if (!CanAccessShipper(user, shipperId))
                return new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status403Forbidden, "You can only access your own assignments");

            var query = await _deliveryRepository.GetAllShipperAssignmentsByShipperIdAsync(shipperId);
            if (query == null || !query.Any())
                return new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status404NotFound, "No shipper assignments found");

            var paged = await query.ToPagedResultAsync(paginationRequest);
            return new ApiResponse<PagedResult<ShipperAssignment>>(StatusCodes.Status200OK, paged);
        }

        public async Task<ApiResponse<ConfirmationResponse>> AcceptOrRejectAssignmentAsync(AcceptAssignmentRequest request, ClaimsPrincipal user)
        {
            if (request.AssignmentId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid assignment id");

            var assignment = await _deliveryRepository.GetShipperAssignmentByIdAsync(request.AssignmentId);
            if (assignment == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No shipper assignment found");

            if (!CanAccessShipper(user, assignment.ShipperId))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status403Forbidden, "You can only respond to your own assignment");

            if (assignment.Status != AssignmentStatus.Pending)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Assignment has already been handled");

            if (request.IsAccepted)
                return await AcceptAssignmentAsync(assignment);

            return await RejectAssignmentAsync(assignment, request);
        }

        public async Task<ApiResponse<ConfirmationResponse>> UpdateAssignmentStatusAsync(Guid assignmentId, UpdateDeliveryStatusRequest request, ClaimsPrincipal user)
        {
            if (assignmentId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid assignment id");

            var assignment = await _deliveryRepository.GetShipperAssignmentByIdAsync(assignmentId);
            if (assignment == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No shipper assignment found");

            if (!CanAccessShipper(user, assignment.ShipperId))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status403Forbidden, "You can only update your own assignment");

            if (request.Status == DeliveryStatus.PickedUp)
                return await ConfirmPickupAsync(assignment, request);

            if (request.Status == DeliveryStatus.Delivered)
                return await ConfirmDeliveryAsync(assignment, request);

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Unsupported delivery status transition");
        }

        public Task<ApiResponse<EstimateDeliveryFeeResponse>> EstimateDeliveryFeeAsync(EstimateDeliveryFeeRequest? request)
        {
            var validationErrors = ValidateEstimateDeliveryFeeRequest(request);
            if (validationErrors.Any())
            {
                return Task.FromResult(new ApiResponse<EstimateDeliveryFeeResponse>(
                    StatusCodes.Status400BadRequest,
                    validationErrors));
            }

            var input = _mapper.ToDeliveryFeeEstimateInput(request!);
            var options = _deliveryOptions.Value;

            var rawDistanceKm = CalculateDistanceKm(input);
            var distanceKm = RoundDistance(rawDistanceKm);
            var baseFee = RoundMoney(Math.Max(options.BaseDeliveryFee, 0m));
            var feePerKm = Math.Max(options.FeePerKm, 0m);
            var distanceFee = RoundMoney(rawDistanceKm * feePerKm);
            var minimumFee = RoundMoney(Math.Max(options.MinimumDeliveryFee, 0m));
            var deliveryFee = RoundMoney(Math.Max(baseFee + distanceFee, minimumFee));
            var maxDeliveryDistanceKm = options.DeliveryRadius > 0 ? (decimal)options.DeliveryRadius : 0m;

            var estimate = new DeliveryFeeEstimate
            {
                PickupLat = input.PickupLat,
                PickupLng = input.PickupLng,
                DeliveryLat = input.DeliveryLat,
                DeliveryLng = input.DeliveryLng,
                DistanceKm = distanceKm,
                EstimatedTimeMinutes = CalculateEstimatedTimeMinutes(rawDistanceKm, options.AverageDeliverySpeedKmPerHour),
                BaseFee = baseFee,
                DistanceFee = distanceFee,
                DeliveryFee = deliveryFee,
                Currency = string.IsNullOrWhiteSpace(options.Currency) ? "VND" : options.Currency,
                IsWithinDeliveryRadius = options.DeliveryRadius <= 0 || rawDistanceKm <= maxDeliveryDistanceKm,
                MaxDeliveryDistanceKm = maxDeliveryDistanceKm
            };

            var response = _mapper.ToEstimateDeliveryFeeResponse(estimate);
            return Task.FromResult(new ApiResponse<EstimateDeliveryFeeResponse>(StatusCodes.Status200OK, response));
        }

        private async Task<ApiResponse<ConfirmationResponse>> AcceptAssignmentAsync(ShipperAssignment assignment)
        {
            var acceptedAssignment = await _deliveryRepository.GetAcceptedShipperAssignmentByOrderIdAsync(assignment.OrderId);
            if (acceptedAssignment != null && acceptedAssignment.Id != assignment.Id)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status409Conflict, "This order has already been accepted by another shipper");

            assignment.Status = AssignmentStatus.Accepted;
            assignment.AcceptedAt = DateTime.UtcNow;
            assignment.RespondedAt = DateTime.UtcNow;
            assignment.RejectReason = null;

            await _deliveryRepository.UpdateShipperAssignment(assignment);
            await UpsertAvailabilityAsync(assignment.ShipperId, assignment.OrderId, ShipperWorkStatus.PendingAssignment);
            await CancelOtherPendingAssignmentsAsync(assignment);

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Accept assignment successfully"));
        }

        private async Task<ApiResponse<ConfirmationResponse>> RejectAssignmentAsync(ShipperAssignment assignment, AcceptAssignmentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RejectReason))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Reject reason is required when rejecting an assignment");

            assignment.Status = AssignmentStatus.Rejected;
            assignment.RespondedAt = DateTime.UtcNow;
            assignment.RejectReason = request.RejectReason.Trim();

            await _deliveryRepository.UpdateShipperAssignment(assignment);

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Reject assignment successfully"));
        }

        private async Task<ApiResponse<ConfirmationResponse>> ConfirmPickupAsync(ShipperAssignment assignment, UpdateDeliveryStatusRequest request)
        {
            if (assignment.Status != AssignmentStatus.Accepted || assignment.PickedUpAt != null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Assignment is not ready for pickup confirmation");

            assignment.PickedUpAt = DateTime.UtcNow;
            assignment.PickupProofFileKey = request.ProofFileKey;

            await UpsertAvailabilityAsync(assignment.ShipperId, assignment.OrderId, ShipperWorkStatus.Delivering);
            await _deliveryRepository.UpdateShipperAssignment(assignment);

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

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Confirm pickup successfully"));
        }

        private async Task<ApiResponse<ConfirmationResponse>> ConfirmDeliveryAsync(ShipperAssignment assignment, UpdateDeliveryStatusRequest request)
        {
            if (assignment.PickedUpAt == null || assignment.DeliveredAt != null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Assignment is not ready for delivery confirmation");

            assignment.DeliveredAt = DateTime.UtcNow;
            assignment.DeliveryProofFileKey = request.ProofFileKey;
            var deliveredAt = assignment.DeliveredAt.Value;

            await UpsertAvailabilityAsync(assignment.ShipperId, null, ShipperWorkStatus.ActiveIdle);
            await _deliveryRepository.UpdateShipperAssignment(assignment);

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

            await _eventPublisher.PublishAsync(new DeliveryDeliveredEvent
            {
                OrderId = assignment.OrderId,
                OrderNumber = assignment.OrderNumber,
                CustomerId = assignment.CustomerId,
                ShipperId = assignment.ShipperId,
                MerchantId = assignment.MerchantId,
                DeliveryFee = assignment.DeliveryFee,
                DistanceKm = assignment.DistanceKm,
                DeliveryAt = deliveredAt,
                Status = DeliveryStatus.Delivered.ToString(),
                ProofFileKey = request.ProofFileKey,
                Note = request.Note
            });

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Confirm delivery successfully"));
        }

        private async Task UpsertAvailabilityAsync(Guid shipperId, Guid? orderId, ShipperWorkStatus status)
        {
            var existingAvailability = await _deliveryRepository.GetShipperAvailabilityByShipperIdAsync(shipperId);
            var availability = existingAvailability ?? new ShipperAvailability
            {
                Id = Guid.NewGuid(),
                ShipperId = shipperId
            };

            availability.CurrentOrderId = orderId;
            availability.Status = status;
            availability.LastSeenAt = DateTime.UtcNow;

            if (existingAvailability == null)
                await _deliveryRepository.CreateShipperAvailabilityAsync(availability);
            else
                await _deliveryRepository.UpdateShipperAvailabilityAsync(availability);
        }

        private async Task CancelOtherPendingAssignmentsAsync(ShipperAssignment acceptedAssignment)
        {
            var otherAssignments = await _deliveryRepository.GetAllShipperAssignmentsByOrderIdAsync(acceptedAssignment.OrderId);
            foreach (var otherAssignment in otherAssignments.Where(item => item != null && item.Id != acceptedAssignment.Id && item.Status == AssignmentStatus.Pending))
            {
                otherAssignment!.Status = AssignmentStatus.Cancelled;
                otherAssignment.RespondedAt = DateTime.UtcNow;
                await _deliveryRepository.UpdateShipperAssignment(otherAssignment);
            }
        }

        private static bool CanAccessAssignment(ClaimsPrincipal user, ShipperAssignment assignment)
        {
            if (IsCurrentUserAdmin(user))
                return true;

            var currentUserId = GetCurrentUserId(user);
            if (currentUserId.HasValue && currentUserId.Value == assignment.CustomerId)
                return true;

            return CanAccessShipper(user, assignment.ShipperId);
        }

        private static bool CanAccessShipper(ClaimsPrincipal user, Guid shipperId)
        {
            if (IsCurrentUserAdmin(user))
                return true;

            var currentShipperId = GetCurrentShipperId(user);
            if (currentShipperId.HasValue)
                return currentShipperId.Value == shipperId;

            return IsCurrentUserInRole(user, "Shipper", "SHIPPER");
        }

        private static Guid? GetCurrentUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? user.FindFirstValue("userId");

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private static Guid? GetCurrentShipperId(ClaimsPrincipal user)
        {
            var shipperIdClaim = user.FindFirstValue("shipperId")
                ?? user.FindFirstValue("ShipperId")
                ?? user.FindFirstValue("shipper_id");

            return Guid.TryParse(shipperIdClaim, out var shipperId) ? shipperId : null;
        }

        private static bool IsCurrentUserAdmin(ClaimsPrincipal user)
        {
            return IsCurrentUserInRole(user, "Admin", "ADMIN");
        }

        private static bool IsCurrentUserInRole(ClaimsPrincipal user, params string[] allowedRoles)
        {
            var roles = user.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Concat(user.FindAll("role").Select(c => c.Value));

            return roles.Any(role => allowedRoles.Any(allowed => string.Equals(role, allowed, StringComparison.OrdinalIgnoreCase)));
        }

        private static List<string> ValidateEstimateDeliveryFeeRequest(EstimateDeliveryFeeRequest? request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("Estimate delivery fee request is required");
                return errors;
            }

            ValidateLatitude(nameof(request.PickupLat), request.PickupLat, errors);
            ValidateLongitude(nameof(request.PickupLng), request.PickupLng, errors);
            ValidateLatitude(nameof(request.DeliveryLat), request.DeliveryLat, errors);
            ValidateLongitude(nameof(request.DeliveryLng), request.DeliveryLng, errors);

            return errors;
        }

        private static void ValidateLatitude(string fieldName, decimal? value, List<string> errors)
        {
            if (!value.HasValue)
            {
                errors.Add($"{fieldName} is required");
                return;
            }

            if (value.Value < -90m || value.Value > 90m)
                errors.Add($"{fieldName} must be between -90 and 90");
        }

        private static void ValidateLongitude(string fieldName, decimal? value, List<string> errors)
        {
            if (!value.HasValue)
            {
                errors.Add($"{fieldName} is required");
                return;
            }

            if (value.Value < -180m || value.Value > 180m)
                errors.Add($"{fieldName} must be between -180 and 180");
        }

        private static decimal CalculateDistanceKm(DeliveryFeeEstimateInput input)
        {
            var pickupLat = ToRadians((double)input.PickupLat);
            var pickupLng = ToRadians((double)input.PickupLng);
            var deliveryLat = ToRadians((double)input.DeliveryLat);
            var deliveryLng = ToRadians((double)input.DeliveryLng);

            var latDelta = deliveryLat - pickupLat;
            var lngDelta = deliveryLng - pickupLng;

            var a = Math.Pow(Math.Sin(latDelta / 2), 2)
                + Math.Cos(pickupLat) * Math.Cos(deliveryLat) * Math.Pow(Math.Sin(lngDelta / 2), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return (decimal)(EarthRadiusKm * c);
        }

        private static double ToRadians(double degree)
        {
            return degree * Math.PI / 180d;
        }

        private static int CalculateEstimatedTimeMinutes(decimal distanceKm, double averageSpeedKmPerHour)
        {
            if (distanceKm <= 0m)
                return 0;

            var speed = averageSpeedKmPerHour > 0 ? averageSpeedKmPerHour : 20d;
            var estimatedMinutes = (double)distanceKm / speed * 60d;

            return Math.Max(1, (int)Math.Ceiling(estimatedMinutes));
        }

        private static decimal RoundDistance(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static decimal RoundMoney(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
