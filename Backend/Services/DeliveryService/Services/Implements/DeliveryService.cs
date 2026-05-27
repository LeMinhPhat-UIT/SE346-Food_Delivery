using DeliveryService.DTOs;
using DeliveryService.Entities;
using DeliveryService.Enums;
using DeliveryService.Exceptions;
using DeliveryService.Integrations;
using DeliveryService.Mappers;
using DeliveryService.Repositories;
using DeliveryService.Repositories.Interfaces;
using DeliveryService.Services.Interfaces;
using Messaging.Contracts.Common;
using Messaging.Contracts.Events;
using Messaging.Contracts.Extensions;
using Messaging.RabbitMq.Publishing;
using System.Security.Claims;

namespace DeliveryService.Services.Implements
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IRedisRepository _redisRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUserServiceClient _userServiceClient;
        private readonly IDeliveryEstimator _deliveryEstimator;
        private readonly DeliveryMapper _mapper;
        private readonly ILogger<DeliveryService> _logger;

        public DeliveryService(
            IDeliveryRepository deliveryRepository,
            IRedisRepository redisRepository,
            IEventPublisher eventPublisher,
            IUserServiceClient userServiceClient,
            IDeliveryEstimator deliveryEstimator,
            DeliveryMapper mapper,
            ILogger<DeliveryService> logger)
        {
            _deliveryRepository = deliveryRepository;
            _redisRepository = redisRepository;
            _eventPublisher = eventPublisher;
            _userServiceClient = userServiceClient;
            _deliveryEstimator = deliveryEstimator;
            _mapper = mapper;
            _logger = logger;
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

            if (request.Lat.HasValue != request.Lng.HasValue)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Both latitude and longitude are required when updating location");

            if (request.Lat.HasValue && !IsValidLatitude(request.Lat.Value))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Lat must be between -90 and 90");

            if (request.Lng.HasValue && !IsValidLongitude(request.Lng.Value))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Lng must be between -180 and 180");

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

            if (request.IsGoOnline && request.Lat.HasValue && request.Lng.HasValue)
                await _redisRepository.UpdateShipperLocationAsync(availability);

            if (!request.IsGoOnline)
                await _redisRepository.DeleteShipperLocationAsync(shipperId);

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

            var validationErrors = ValidateCoordinates(request.Latitude, request.Longitude);
            if (validationErrors.Any())
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, validationErrors);

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

            await _redisRepository.UpdateShipperLocationAsync(availability);
            await AddLocationHistoryAsync(request.OrderId, request.ShipperId, request.Latitude, request.Longitude);

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Update location successfully"));
        }

        public async Task<ApiResponse<ConfirmationResponse>> UpdateShipperLocationAsync(Guid shipperId, UpdateShipperLocationRequest request, ClaimsPrincipal user)
        {
            if (shipperId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid shipper id");

            if (!CanAccessShipper(user, shipperId))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status403Forbidden, "You can only update your own shipper location");

            var validationErrors = ValidateCoordinates(request.Latitude, request.Longitude);
            if (validationErrors.Any())
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, validationErrors);

            var normalizedOrderId = request.OrderId.GetValueOrDefault();
            var hasOrder = normalizedOrderId != Guid.Empty;
            var existingAvailability = await _deliveryRepository.GetShipperAvailabilityByShipperIdAsync(shipperId);

            if (existingAvailability != null && existingAvailability.Status == ShipperWorkStatus.Offline)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Shipper must be online before updating location");

            if (!hasOrder &&
                existingAvailability != null &&
                (existingAvailability.Status == ShipperWorkStatus.PendingAssignment || existingAvailability.Status == ShipperWorkStatus.Delivering))
            {
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Order id is required while shipper has an active assignment");
            }

            var availability = existingAvailability ?? new ShipperAvailability
            {
                Id = Guid.NewGuid(),
                ShipperId = shipperId,
                Status = ShipperWorkStatus.ActiveIdle
            };

            availability.CurrentOrderId = hasOrder ? normalizedOrderId : null;
            availability.CurrentLat = request.Latitude;
            availability.CurrentLng = request.Longitude;
            availability.LastSeenAt = DateTime.UtcNow;

            if (existingAvailability == null)
                await _deliveryRepository.CreateShipperAvailabilityAsync(availability);
            else
                await _deliveryRepository.UpdateShipperAvailabilityAsync(availability);

            await _redisRepository.UpdateShipperLocationAsync(availability);

            if (hasOrder)
                await AddLocationHistoryAsync(normalizedOrderId, shipperId, request.Latitude, request.Longitude);

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Update shipper location successfully"));
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

            if (request.IsAccepted)
                return await AcceptAssignmentAsync(request.AssignmentId, request, user);

            return await RejectAssignmentAsync(
                request.AssignmentId,
                new RejectAssignmentRequest
                {
                    OfferId = request.OfferId,
                    Reason = request.RejectReason
                },
                user);
        }

        public async Task<ApiResponse<ConfirmationResponse>> AcceptAssignmentAsync(Guid assignmentId, AcceptAssignmentRequest request, ClaimsPrincipal user)
        {
            if (assignmentId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid assignment id");

            if (request.OfferId.HasValue && request.OfferId.Value != assignmentId)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Offer id does not match assignment id");

            var shipperId = await ResolveCurrentShipperIdAsync(user);
            if (!shipperId.HasValue)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status403Forbidden, "SHIPPER_NOT_ELIGIBLE");

            var now = DateTime.UtcNow;
            var result = await _deliveryRepository.AcceptAssignmentOfferAsync(assignmentId, shipperId.Value, now);

            if (result.Outcome == AssignmentAcceptanceOutcome.Accepted && result.Assignment != null)
            {
                await _eventPublisher.PublishAsync(new AssignmentAcceptedEvent
                {
                    CorrelationId = result.Assignment.OrderId.ToString(),
                    AssignmentId = result.Assignment.Id,
                    OfferId = result.Assignment.Id,
                    OrderId = result.Assignment.OrderId,
                    OrderNumber = result.Assignment.OrderNumber,
                    CustomerId = result.Assignment.CustomerId,
                    MerchantId = result.Assignment.MerchantId,
                    AcceptedByShipperId = result.Assignment.ShipperId,
                    CancelledOfferIds = result.CancelledAssignments.Select(assignment => assignment.Id).ToArray(),
                    CancelledShipperIds = result.CancelledAssignments.Select(assignment => assignment.ShipperId).ToArray()
                });

                return new ApiResponse<ConfirmationResponse>(
                    StatusCodes.Status200OK,
                    new ConfirmationResponse("Assignment accepted successfully."));
            }

            return result.Outcome switch
            {
                AssignmentAcceptanceOutcome.NotFound => new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "OFFER_NOT_FOUND"),
                AssignmentAcceptanceOutcome.OfferNotFound => new ApiResponse<ConfirmationResponse>(StatusCodes.Status403Forbidden, "OFFER_NOT_FOUND"),
                AssignmentAcceptanceOutcome.OfferExpired => new ApiResponse<ConfirmationResponse>(StatusCodes.Status409Conflict, "OFFER_EXPIRED"),
                AssignmentAcceptanceOutcome.AlreadyTaken => new ApiResponse<ConfirmationResponse>(StatusCodes.Status409Conflict, "ASSIGNMENT_ALREADY_TAKEN"),
                _ => new ApiResponse<ConfirmationResponse>(StatusCodes.Status409Conflict, "Assignment is no longer available.")
            };
        }

        public async Task<ApiResponse<ConfirmationResponse>> RejectAssignmentAsync(Guid assignmentId, RejectAssignmentRequest request, ClaimsPrincipal user)
        {
            if (assignmentId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid assignment id");

            if (request.OfferId.HasValue && request.OfferId.Value != assignmentId)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Offer id does not match assignment id");

            if (string.IsNullOrWhiteSpace(request.Reason))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Reject reason is required when rejecting an assignment");

            var shipperId = await ResolveCurrentShipperIdAsync(user);
            if (!shipperId.HasValue)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status403Forbidden, "SHIPPER_NOT_ELIGIBLE");

            var rejected = await _deliveryRepository.RejectAssignmentOfferAsync(
                assignmentId,
                shipperId.Value,
                request.Reason.Trim(),
                DateTime.UtcNow);

            if (rejected == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status409Conflict, "Assignment offer is no longer available.");

            await _eventPublisher.PublishAsync(new AssignmentRejectedEvent
            {
                CorrelationId = rejected.OrderId.ToString(),
                AssignmentId = rejected.Id,
                OfferId = rejected.Id,
                OrderId = rejected.OrderId,
                OrderNumber = rejected.OrderNumber,
                ShipperId = rejected.ShipperId,
                Reason = rejected.RejectReason ?? request.Reason.Trim(),
                RejectedAt = rejected.RespondedAt ?? DateTime.UtcNow
            });

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Assignment rejected successfully."));
        }

        public async Task<ApiResponse<ActiveAssignmentOfferResponse>> GetActiveOfferAsync(ClaimsPrincipal user)
        {
            var shipperId = await ResolveCurrentShipperIdAsync(user);
            if (!shipperId.HasValue)
                return new ApiResponse<ActiveAssignmentOfferResponse>(StatusCodes.Status403Forbidden, "SHIPPER_NOT_ELIGIBLE");

            var offer = await _deliveryRepository.GetActiveOfferForShipperAsync(shipperId.Value);
            if (offer == null)
            {
                return new ApiResponse<ActiveAssignmentOfferResponse>(
                    StatusCodes.Status200OK,
                    new ActiveAssignmentOfferResponse { HasActiveOffer = false });
            }

            return new ApiResponse<ActiveAssignmentOfferResponse>(
                StatusCodes.Status200OK,
                new ActiveAssignmentOfferResponse
                {
                    HasActiveOffer = true,
                    AssignmentId = offer.Id,
                    OfferId = offer.Id,
                    OrderId = offer.OrderId,
                    ExpiresAt = offer.OfferExpiresAt
                });
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

        public async Task<ApiResponse<EstimateDeliveryFeeResponse>> EstimateDeliveryFeeAsync(EstimateDeliveryFeeRequest? request)
        {
            var validationErrors = ValidateEstimateDeliveryFeeRequest(request);
            if (validationErrors.Any())
            {
                return new ApiResponse<EstimateDeliveryFeeResponse>(
                    StatusCodes.Status400BadRequest,
                    validationErrors);
            }

            var input = _mapper.ToDeliveryFeeEstimateInput(request!);

            try
            {
                var estimate = await _deliveryEstimator.EstimateAsync(input);
                var response = _mapper.ToEstimateDeliveryFeeResponse(estimate);
                return new ApiResponse<EstimateDeliveryFeeResponse>(StatusCodes.Status200OK, response);
            }
            catch (OpenRouteServiceException ex)
            {
                _logger.LogWarning(ex, "Unable to estimate delivery fee from OpenRouteService");
                return new ApiResponse<EstimateDeliveryFeeResponse>(
                    StatusCodes.Status502BadGateway,
                    "Unable to estimate delivery route at the moment");
            }
        }

        public async Task<ApiResponse<PagedResult<ShipperLocationHistory>>> GetLocationHistoryByOrderIdAsync(Guid orderId, PaginationRequest paginationRequest, ClaimsPrincipal user)
        {
            if (orderId == Guid.Empty)
                return new ApiResponse<PagedResult<ShipperLocationHistory>>(StatusCodes.Status400BadRequest, "Invalid order id");

            if (!IsCurrentUserAdmin(user))
            {
                var assignments = await _deliveryRepository.GetAllShipperAssignmentsByOrderIdAsync(orderId);
                if (!assignments.Any(assignment => CanAccessAssignment(user, assignment)))
                    return new ApiResponse<PagedResult<ShipperLocationHistory>>(StatusCodes.Status403Forbidden, "You can only access location history for your own orders or assignments");
            }

            var query = await _deliveryRepository.GetAllShipperLocationHistoriesByOrderIdAsync(orderId);
            if (!query.Any())
                return new ApiResponse<PagedResult<ShipperLocationHistory>>(StatusCodes.Status404NotFound, "No shipper location history found");

            var paged = await query.ToPagedResultAsync(paginationRequest);
            return new ApiResponse<PagedResult<ShipperLocationHistory>>(StatusCodes.Status200OK, paged);
        }

        public async Task<ApiResponse<PagedResult<ShipperLocationHistory>>> GetLocationHistoryByShipperIdAsync(Guid shipperId, PaginationRequest paginationRequest, ClaimsPrincipal user)
        {
            if (shipperId == Guid.Empty)
                return new ApiResponse<PagedResult<ShipperLocationHistory>>(StatusCodes.Status400BadRequest, "Invalid shipper id");

            if (!CanAccessShipper(user, shipperId))
                return new ApiResponse<PagedResult<ShipperLocationHistory>>(StatusCodes.Status403Forbidden, "You can only access your own shipper location history");

            var query = await _deliveryRepository.GetAllShipperLocationHistoriesByShipperIdAsync(shipperId);
            if (!query.Any())
                return new ApiResponse<PagedResult<ShipperLocationHistory>>(StatusCodes.Status404NotFound, "No shipper location history found");

            var paged = await query.ToPagedResultAsync(paginationRequest);
            return new ApiResponse<PagedResult<ShipperLocationHistory>>(StatusCodes.Status200OK, paged);
        }

        public async Task<ApiResponse<ConfirmationResponse>> ReportIncidentAsync(ReportIncidentRequest request, ClaimsPrincipal user)
        {
            var reporterId = GetCurrentUserId(user);
            if (!reporterId.HasValue)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status401Unauthorized, "Invalid user context");

            if (request.OrderId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid order id");

            if (string.IsNullOrWhiteSpace(request.Description))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Description is required");

            var incident = new Incident
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                ReportedBy = reporterId.Value,
                Type = request.Type,
                Description = request.Description.Trim(),
                ProofUrl = request.ProofUrls
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => url.Trim())
                    .ToArray(),
                Status = IncidentStatus.Pending,
                Resolution = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _deliveryRepository.CreateIncidentAsync(incident);

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Report incident successfully"));
        }

        public async Task<ApiResponse<PagedResult<Incident>>> GetAllIncidentsAsync(PaginationRequest paginationRequest)
        {
            var query = await _deliveryRepository.GetAllIncidentsAsync();
            if (!query.Any())
                return new ApiResponse<PagedResult<Incident>>(StatusCodes.Status404NotFound, "No incidents found");

            var paged = await query.ToPagedResultAsync(paginationRequest);
            return new ApiResponse<PagedResult<Incident>>(StatusCodes.Status200OK, paged);
        }

        public async Task<ApiResponse<PagedResult<Incident>>> GetIncidentsByReporterIdAsync(Guid reporterId, PaginationRequest paginationRequest, ClaimsPrincipal user)
        {
            if (reporterId == Guid.Empty)
                return new ApiResponse<PagedResult<Incident>>(StatusCodes.Status400BadRequest, "Invalid reporter id");

            if (!CanAccessUser(user, reporterId))
                return new ApiResponse<PagedResult<Incident>>(StatusCodes.Status403Forbidden, "You can only access your own incidents");

            var query = await _deliveryRepository.GetAllIncidentByReporterId(reporterId);
            if (!query.Any())
                return new ApiResponse<PagedResult<Incident>>(StatusCodes.Status404NotFound, "No incidents found");

            var paged = await query.ToPagedResultAsync(paginationRequest);
            return new ApiResponse<PagedResult<Incident>>(StatusCodes.Status200OK, paged);
        }

        public async Task<ApiResponse<Incident>> GetIncidentByIdAsync(Guid incidentId, ClaimsPrincipal user)
        {
            if (incidentId == Guid.Empty)
                return new ApiResponse<Incident>(StatusCodes.Status400BadRequest, "Invalid incident id");

            var incident = await _deliveryRepository.GetIncidentByIdAsync(incidentId);
            if (incident == null)
                return new ApiResponse<Incident>(StatusCodes.Status404NotFound, "No incident found");

            if (!CanAccessUser(user, incident.ReportedBy))
                return new ApiResponse<Incident>(StatusCodes.Status403Forbidden, "You can only access your own incidents");

            return new ApiResponse<Incident>(StatusCodes.Status200OK, incident);
        }

        public async Task<ApiResponse<ConfirmationResponse>> ResolveIncidentAsync(Guid incidentId, ResolveIncidentRequest request, ClaimsPrincipal user)
        {
            if (!IsCurrentUserAdmin(user))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status403Forbidden, "Only admin can resolve incidents");

            var resolvedBy = GetCurrentUserId(user);
            if (!resolvedBy.HasValue)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status401Unauthorized, "Invalid user context");

            if (incidentId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid incident id");

            if (request.Status == IncidentStatus.Pending)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Resolved incident status can not be pending");

            if (string.IsNullOrWhiteSpace(request.Resolution))
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Resolution is required");

            var incident = await _deliveryRepository.GetIncidentByIdAsync(incidentId);
            if (incident == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No incident found");

            incident.Status = request.Status;
            incident.Resolution = request.Resolution.Trim();
            incident.ResolvedBy = resolvedBy.Value;
            incident.ResolvedAt = DateTime.UtcNow;
            incident.UpdatedAt = DateTime.UtcNow;

            await _deliveryRepository.UpdateIncidentAsync(incident);

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Resolve incident successfully"));
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

            assignment.Status = AssignmentStatus.PickedUp;
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

            assignment.Status = AssignmentStatus.Completed;
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
            availability.CurrentAssignmentId = status == ShipperWorkStatus.ActiveIdle ? null : availability.CurrentAssignmentId;
            availability.CurrentOfferedAssignmentId = null;
            availability.OfferingExpiresAt = null;
            availability.LastSeenAt = DateTime.UtcNow;

            if (existingAvailability == null)
                await _deliveryRepository.CreateShipperAvailabilityAsync(availability);
            else
                await _deliveryRepository.UpdateShipperAvailabilityAsync(availability);
        }

        private async Task AddLocationHistoryAsync(Guid orderId, Guid shipperId, decimal latitude, decimal longitude)
        {
            await _deliveryRepository.AddShipperLocationHistoriesAsync(new[]
            {
                new ShipperLocationHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ShipperId = shipperId,
                    Latitude = latitude,
                    Longitude = longitude,
                    RecordedAt = DateTime.UtcNow
                }
            });
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

        private static bool CanAccessUser(ClaimsPrincipal user, Guid userId)
        {
            if (IsCurrentUserAdmin(user))
                return true;

            var currentUserId = GetCurrentUserId(user);
            return currentUserId.HasValue && currentUserId.Value == userId;
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

        private async Task<Guid?> ResolveCurrentShipperIdAsync(ClaimsPrincipal user)
        {
            var shipperId = GetCurrentShipperId(user);
            if (shipperId.HasValue)
                return shipperId;

            var userId = GetCurrentUserId(user);
            if (!userId.HasValue)
                return null;

            if (!IsCurrentUserInRole(user, "Shipper", "SHIPPER") && !IsCurrentUserAdmin(user))
                return null;

            return await _userServiceClient.GetShipperIdByUserIdAsync(userId.Value);
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

        private static List<string> ValidateCoordinates(decimal latitude, decimal longitude)
        {
            var errors = new List<string>();

            if (!IsValidLatitude(latitude))
                errors.Add("Latitude must be between -90 and 90");

            if (!IsValidLongitude(longitude))
                errors.Add("Longitude must be between -180 and 180");

            return errors;
        }

        private static bool IsValidLatitude(decimal value)
        {
            return value >= -90m && value <= 90m;
        }

        private static bool IsValidLongitude(decimal value)
        {
            return value >= -180m && value <= 180m;
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

    }
}
