using Messaging.Contracts.Common;
using Messaging.Contracts.Events;
using Messaging.Contracts.Extensions;
using UserService.DTOs.ShipperDTOs;
using UserService.Entities;
using UserService.Enums;

namespace UserService.Services.Implements
{
    public partial class UserService
    {
        public async Task<ApiResponse<ConfirmationResponse>> RequestForShipperRole(Guid userId, CreateShipperRequest request)
        {
            if (userId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid user id");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No user found");

            var existingShipper = await _userRepository.GetShipperByUserIdAsync(userId);
            if (existingShipper != null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status409Conflict, "You are already a shipper");

            var existingPendingShipper = await _userRepository.GetPendingShipperRequestByUserIdAsync(userId);
            if (existingPendingShipper != null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status409Conflict, "You already have a pending shipper request");

            var shipperRequest = _mapper.ToShipperRequest(request);
            shipperRequest.UserId = userId;
            shipperRequest.VerificationStatus = VerificationStatus.Pending;
            shipperRequest.RejectedReason = string.Empty;
            shipperRequest.ReviewedBy = userId;
            shipperRequest.VerifiedAt = null;
            shipperRequest.CreatedAt = DateTime.UtcNow;

            var result = await _userRepository.CreateShipperRequest(shipperRequest);

            if (!result)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Can not create request for shipper role");

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Create request for shipper role successfully"));
        }

        public async Task<ApiResponse<PagedResult<ShipperRequestResponse>>> GetAllShipperRequestsAsync(PaginationRequest paginationRequest)
        {
            var shipperRequests = await _userRepository.GetAllShipperRequestAsync();
            var pagedShipperRequests = await shipperRequests.ToPagedResultAsync(paginationRequest);

            var response = _mapper.ToShipperRequestResponseList(pagedShipperRequests.Items)
                .Select(s =>
                {
                    if (s.ReviewedBy == Guid.Empty)
                        s.ReviewedBy = null;

                    return s;
                });

            return new ApiResponse<PagedResult<ShipperRequestResponse>>(
                StatusCodes.Status200OK,
                new PagedResult<ShipperRequestResponse>(response));
        }

        public async Task<ApiResponse<ShipperResponse>> GetShipperByIdAsync(Guid shipperId)
        {
            if (shipperId == Guid.Empty)
                return new ApiResponse<ShipperResponse>(StatusCodes.Status400BadRequest, "Invalid shipper id");

            var shipper = await _userRepository.GetShipperByIdAsync(shipperId);

            if (shipper == null)
                return new ApiResponse<ShipperResponse>(StatusCodes.Status404NotFound, "No shipper found");

            var response = _mapper.ToShipperResponse(shipper);

            return new ApiResponse<ShipperResponse>(StatusCodes.Status200OK, response);
        }

        public async Task<ApiResponse<PagedResult<ShipperResponse>>> GetAllShippersAsync(PaginationRequest paginationRequest)
        {
            var shippers = await _userRepository.GetAllShippersAsync();
            var pagedShippers = await shippers.ToPagedResultAsync(paginationRequest);

            var response = _mapper.ToShipperResponseList(pagedShippers.Items);

            return new ApiResponse<PagedResult<ShipperResponse>>(
                StatusCodes.Status200OK,
                new PagedResult<ShipperResponse>(response));
        }

        public async Task<ApiResponse<ConfirmationResponse>> UpdateShipperAsync(Guid shipperId, UpdateShipperRequest request)
        {
            if (shipperId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid shipper id");

            var shipper = await _userRepository.GetShipperByIdAsync(shipperId);
            if (shipper == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No shipper found");

            if (!string.IsNullOrWhiteSpace(request.VehiclePlate))
                shipper.VehiclePlate = request.VehiclePlate.Trim();

            if (request.Status.HasValue)
                shipper.Status = request.Status.Value;

            shipper.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateShipperAsync(shipper);

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Update shipper successfully"));
        }

        public async Task<ApiResponse<ConfirmationResponse>> DeleteShipperAsync(Guid shipperId)
        {
            if (shipperId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid shipper id");

            var isDeleted = await _userRepository.DeleteShipperAsync(shipperId);
            if (!isDeleted)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No shipper found");

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Delete shipper successfully"));
        }

        public async Task<ApiResponse<ConfirmationResponse>> ReviewShipperRequestAsync(Guid requestId, Guid reviewerId, ReviewShipperRequest request)
        {
            if (requestId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid shipper request id");

            if (reviewerId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid reviewer id");

            var reviewer = await _userRepository.GetUserByIdAsync(reviewerId);
            if (reviewer == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No reviewer found");

            var shipperRequest = await _userRepository.GetShipperRequestByIdAsync(requestId);
            if (shipperRequest == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No shipper request found");

            if (shipperRequest.VerificationStatus != VerificationStatus.Pending)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Shipper request has already been reviewed");

            if (request.VerificationStatus == VerificationStatus.Pending)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Review status can not be pending");

            shipperRequest.VerificationStatus = request.VerificationStatus;
            shipperRequest.ReviewedBy = reviewerId;
            shipperRequest.VerifiedAt = DateTime.UtcNow;

            if (request.VerificationStatus == VerificationStatus.Rejected)
            {
                if (string.IsNullOrWhiteSpace(request.RejectedReason))
                    return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Rejected reason is required when rejecting request");

                shipperRequest.RejectedReason = request.RejectedReason.Trim();
                await _userRepository.UpdateShipperRequestAsync(shipperRequest);

                var rejectedEvent = new ShipperRequestReviewedEvent()
                {
                    RequestId = requestId,
                    UserId = shipperRequest.UserId,
                    ReviewerId = reviewerId,
                    IsApproved = false,
                    RejectedReason = request.RejectedReason
                };

                await _eventPublisher.PublishAsync(rejectedEvent);

                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Shipper request rejected successfully"));
            }

            var existingShipper = await _userRepository.GetShipperByUserIdAsync(shipperRequest.UserId);
            if (existingShipper != null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status409Conflict, "Shipper is already created for this user");

            var shipper = _mapper.ToShipper(shipperRequest);
            shipper.VehiclePlate = string.Empty;
            shipper.Status = ShipperStatus.Approved;
            shipper.CreatedAt = DateTime.UtcNow;

            var created = await _userRepository.CreateShipperAsync(shipper);
            if (!created)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Can not create shipper");

            shipperRequest.RejectedReason = string.Empty;

            await _userRepository.UpdateShipperRequestAsync(shipperRequest);

            var approvedEvent = new ShipperRequestReviewedEvent()
            {
                RequestId = requestId,
                UserId = shipperRequest.UserId,
                ReviewerId = reviewerId,
                IsApproved = true,
                RejectedReason = request.RejectedReason
            };

            await _eventPublisher.PublishAsync(approvedEvent);

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Shipper request approved successfully"));
        }
    }
}
