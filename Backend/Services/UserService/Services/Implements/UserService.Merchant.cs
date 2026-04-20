using Messaging.Contracts.Common;
using Messaging.Contracts.Extensions;
using UserService.DTOs;
using UserService.Entities;
using UserService.Enums;

namespace UserService.Services.Implements
{
    public partial class UserService
    {
        public async Task<ApiResponse<ConfirmationResponse>> RequestForMerchantRole(Guid userId, CreateMerchantRequest request)
        {
            if (userId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid user id");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No user found");

            var existingMerchant = await _userRepository.GetMerchantByUserIdAsync(userId);
            if (existingMerchant != null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status409Conflict, "You are already a merchant");

            var existingPendingRequest = await _userRepository.GetPendingMerchantRequestByUserIdAsync(userId);
            if (existingPendingRequest != null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status409Conflict, "You already have a pending merchant request");

            var merchantRequest = _mapper.ToMerchantRequest(request);
            merchantRequest.UserId = userId;
            merchantRequest.VerificationStatus = VerificationStatus.Pending;
            merchantRequest.RejectedReason = string.Empty;
            merchantRequest.ReviewedBy = userId;
            merchantRequest.VerifiedAt = null;
            merchantRequest.CreatedAt = DateTime.UtcNow;

            var result = await _userRepository.CreateMerchantRequest(merchantRequest);

            if (!result)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Can not create request for merchant role");

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Create request for merchant role successfully"));
        }

        public async Task<ApiResponse<PagedResult<MerchantRequestResponse>>> GetAllMerchantRequests(PaginationRequest paginationRequest)
        {
            var merchantRequests = await _userRepository.GetAllMerchantRequestAsync();
            var pagedMerchantRequests = await merchantRequests.ToPagedResultAsync(paginationRequest);

            var response = _mapper.ToMerchantRequestResponseList(pagedMerchantRequests.Items)
                .Select(r =>
                {
                    if (r.ReviewedBy == Guid.Empty)
                        r.ReviewedBy = null;

                    return r;
                });

            return new ApiResponse<PagedResult<MerchantRequestResponse>>(
                StatusCodes.Status200OK, 
                new PagedResult<MerchantRequestResponse>(response));
        }

        public async Task<ApiResponse<ConfirmationResponse>> ReviewMerchantRequestAsync(Guid requestId, Guid reviewerId, ReviewMerchantRequest request)
        {
            if (requestId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid merchant request id");

            if (reviewerId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid reviewer id");

            var reviewer = await _userRepository.GetUserByIdAsync(reviewerId);
            if (reviewer == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No reviewer found");

            var merchantRequest = await _userRepository.GetMerchantRequestByIdAsync(requestId);
            if (merchantRequest == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No merchant request found");

            if (merchantRequest.VerificationStatus != VerificationStatus.Pending)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Merchant request has already been reviewed");

            if (request.VerificationStatus == VerificationStatus.Pending)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Review status can not be pending");

            merchantRequest.VerificationStatus = request.VerificationStatus;
            merchantRequest.ReviewedBy = reviewerId;
            merchantRequest.VerifiedAt = DateTime.UtcNow;

            if (request.VerificationStatus == VerificationStatus.Rejected)
            {
                if (string.IsNullOrWhiteSpace(request.RejectedReason))
                    return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Rejected reason is required when rejecting request");

                merchantRequest.RejectedReason = request.RejectedReason.Trim();
                await _userRepository.UpdateMerchantRequestAsync(merchantRequest);

                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Merchant request rejected successfully"));
            }

            var existingMerchant = await _userRepository.GetMerchantByUserIdAsync(merchantRequest.UserId);
            if (existingMerchant != null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status409Conflict, "Merchant is already created for this user");

            var merchant = _mapper.ToMerchant(merchantRequest);
            merchant.StoreLogoUrl = string.Empty;
            merchant.StoreBannerUrl = string.Empty;
            merchant.Status = MerchantStatus.Approved;
            merchant.CreatedAt = DateTime.UtcNow;
            merchant.IsOpen = true;
            merchant.Addresses = new List<MerchantAddress>();

            var created = await _userRepository.CreateMerchantAsync(merchant);
            if (!created)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Can not create merchant");

            merchantRequest.RejectedReason = string.Empty;
            await _userRepository.UpdateMerchantRequestAsync(merchantRequest);

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Merchant request approved successfully"));
        }

        public async Task<ApiResponse<PagedResult<MerchantResponse>>> GetAllMerchantsAsync(PaginationRequest paginationRequest)
        {
            var merchants = await _userRepository.GetAllMerchantsAsync();
            var pagedMerchants = await merchants.ToPagedResultAsync(paginationRequest);

            var response = _mapper.ToMerchantResponseList(pagedMerchants.Items);

            return new ApiResponse<PagedResult<MerchantResponse>>(
                StatusCodes.Status200OK,
                new PagedResult<MerchantResponse>(response));
        }

        public async Task<ApiResponse<MerchantResponse>> GetMerchantByIdAsync(Guid merchantId)
        {
            if (merchantId == Guid.Empty)
                return new ApiResponse<MerchantResponse>(StatusCodes.Status400BadRequest, "Invalid merchant id");

            var merchant = await _userRepository.GetMerchantByIdAsync(merchantId);
            if (merchant == null)
                return new ApiResponse<MerchantResponse>(StatusCodes.Status404NotFound, "No merchant found");

            var response = _mapper.ToMerchantResponse(merchant);

            return new ApiResponse<MerchantResponse>(StatusCodes.Status200OK, response);
        }

        public async Task<ApiResponse<ConfirmationResponse>> UpdateMerchantAsync(Guid merchantId, UpdateMerchantRequest request)
        {
            if (merchantId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid merchant id");

            var merchant = await _userRepository.GetMerchantByIdAsync(merchantId);
            if (merchant == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No merchant found");

            if (!string.IsNullOrWhiteSpace(request.StoreName))
                merchant.StoreName = request.StoreName.Trim();

            if (!string.IsNullOrWhiteSpace(request.StoreDescription))
                merchant.StoreDescription = request.StoreDescription.Trim();

            if (!string.IsNullOrWhiteSpace(request.StoreLogoUrl))
                merchant.StoreLogoUrl = request.StoreLogoUrl.Trim();

            if (!string.IsNullOrWhiteSpace(request.StoreBannerUrl))
                merchant.StoreBannerUrl = request.StoreBannerUrl.Trim();

            if (!string.IsNullOrWhiteSpace(request.BusinessLicense))
                merchant.BusinessLicense = request.BusinessLicense.Trim();

            if (!string.IsNullOrWhiteSpace(request.TaxId))
                merchant.TaxId = request.TaxId.Trim();

            if (request.IsOpen.HasValue)
                merchant.IsOpen = request.IsOpen.Value;

            if (request.OpeningTime.HasValue)
                merchant.OpeningTime = request.OpeningTime;

            if (request.ClosingTime.HasValue)
                merchant.ClosingTime = request.ClosingTime;

            if (request.MinOrderAmount.HasValue)
                merchant.MinOrderAmount = request.MinOrderAmount;

            if (request.AvgPrepTime.HasValue)
                merchant.AvgPrepTime = request.AvgPrepTime;

            if (request.Status.HasValue)
                merchant.Status = request.Status.Value;

            merchant.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateMerchantAsync(merchant);

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Update merchant successfully"));
        }

        public async Task<ApiResponse<ConfirmationResponse>> DeleteMerchantAsync(Guid merchantId)
        {
            if (merchantId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid merchant id");

            var isDeleted = await _userRepository.DeleteMerchantAsync(merchantId);
            if (!isDeleted)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No merchant found");

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Delete merchant successfully"));
        }
    }
}
