using DeliveryService.DTOs;
using DeliveryService.Entities;
using DeliveryService.Repositories.Interfaces;
using DeliveryService.Services.Interfaces;
using Messaging.Contracts.Common;
using Messaging.Contracts.Extensions;

namespace DeliveryService.Services.Implements
{
    public class DeliveryFeePolicyService : IDeliveryFeePolicyService
    {
        private readonly IDeliveryRepository _deliveryRepository;

        public DeliveryFeePolicyService(IDeliveryRepository deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        public async Task<ApiResponse<PagedResult<DeliveryFeePolicyResponse>>> GetPoliciesAsync(
            PaginationRequest paginationRequest,
            bool includeInactive,
            CancellationToken cancellationToken = default)
        {
            var query = await _deliveryRepository.GetAllDeliveryFeePoliciesAsync(includeInactive);
            var pagedPolicies = await query.ToPagedResultAsync(paginationRequest, cancellationToken);
            if (pagedPolicies.TotalCount == 0)
                return new ApiResponse<PagedResult<DeliveryFeePolicyResponse>>(StatusCodes.Status404NotFound, "No delivery fee policies found");

            var response = new PagedResult<DeliveryFeePolicyResponse>(
                pagedPolicies.Items.Select(MapPolicy),
                pagedPolicies.PaginationRequest,
                pagedPolicies.TotalCount);

            return new ApiResponse<PagedResult<DeliveryFeePolicyResponse>>(StatusCodes.Status200OK, response);
        }

        public async Task<ApiResponse<DeliveryFeePolicyResponse>> GetPolicyAsync(
            Guid policyId,
            CancellationToken cancellationToken = default)
        {
            if (policyId == Guid.Empty)
                return new ApiResponse<DeliveryFeePolicyResponse>(StatusCodes.Status400BadRequest, "Invalid delivery fee policy id");

            var policy = await _deliveryRepository.GetDeliveryFeePolicyByIdAsync(policyId, cancellationToken);

            if (policy == null)
                return new ApiResponse<DeliveryFeePolicyResponse>(StatusCodes.Status404NotFound, "Delivery fee policy not found");

            return new ApiResponse<DeliveryFeePolicyResponse>(StatusCodes.Status200OK, MapPolicy(policy));
        }

        public async Task<ApiResponse<DeliveryFeePolicyResponse>> CreatePolicyAsync(
            DeliveryFeePolicyRequest? request,
            CancellationToken cancellationToken = default)
        {
            var validationErrors = ValidatePolicyRequest(request);
            if (validationErrors.Any())
                return new ApiResponse<DeliveryFeePolicyResponse>(StatusCodes.Status400BadRequest, validationErrors);

            var policy = BuildPolicy(request!, DateTime.UtcNow);

            await _deliveryRepository.CreateDeliveryFeePolicyAsync(policy, cancellationToken);

            return new ApiResponse<DeliveryFeePolicyResponse>(StatusCodes.Status201Created, MapPolicy(policy));
        }

        public async Task<ApiResponse<DeliveryFeePolicyResponse>> UpdatePolicyAsync(
            Guid policyId,
            DeliveryFeePolicyRequest? request,
            CancellationToken cancellationToken = default)
        {
            if (policyId == Guid.Empty)
                return new ApiResponse<DeliveryFeePolicyResponse>(StatusCodes.Status400BadRequest, "Invalid delivery fee policy id");

            var validationErrors = ValidatePolicyRequest(request);
            if (validationErrors.Any())
                return new ApiResponse<DeliveryFeePolicyResponse>(StatusCodes.Status400BadRequest, validationErrors);

            var existingPolicy = await _deliveryRepository.GetDeliveryFeePolicyByIdAsync(policyId, cancellationToken);

            if (existingPolicy == null)
                return new ApiResponse<DeliveryFeePolicyResponse>(StatusCodes.Status404NotFound, "Delivery fee policy not found");

            var hasHistoricalQuotes = await _deliveryRepository.HasDeliveryFeeQuoteDetailsForPolicyAsync(policyId, cancellationToken);

            if (hasHistoricalQuotes)
            {
                var replacement = BuildPolicy(request!, DateTime.UtcNow);
                existingPolicy.IsActive = false;
                existingPolicy.UpdatedAt = DateTime.UtcNow;

                await _deliveryRepository.ReplaceUsedDeliveryFeePolicyAsync(existingPolicy, replacement, cancellationToken);

                return new ApiResponse<DeliveryFeePolicyResponse>(StatusCodes.Status200OK, MapPolicy(replacement));
            }

            ApplyPolicyRequest(existingPolicy, request!, DateTime.UtcNow);
            var replacementTiers = BuildDistanceTiers(existingPolicy.Id, request!.DistanceTiers);
            await _deliveryRepository.UpdateDeliveryFeePolicyAsync(existingPolicy, replacementTiers, cancellationToken);

            return new ApiResponse<DeliveryFeePolicyResponse>(StatusCodes.Status200OK, MapPolicy(existingPolicy));
        }

        public async Task<ApiResponse<ConfirmationResponse>> DeletePolicyAsync(
            Guid policyId,
            CancellationToken cancellationToken = default)
        {
            if (policyId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid delivery fee policy id");

            var policy = await _deliveryRepository.GetDeliveryFeePolicyByIdAsync(policyId, cancellationToken);

            if (policy == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "Delivery fee policy not found");

            await _deliveryRepository.SoftDeleteDeliveryFeePolicyAsync(policy, DateTime.UtcNow, cancellationToken);

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Delete delivery fee policy successfully"));
        }

        private static DeliveryFeePolicy BuildPolicy(DeliveryFeePolicyRequest request, DateTime now)
        {
            var policyId = Guid.NewGuid();
            var policy = new DeliveryFeePolicy
            {
                Id = policyId,
                CreatedAt = now
            };

            ApplyPolicyRequest(policy, request, null);
            policy.DistanceTiers = BuildDistanceTiers(policyId, request.DistanceTiers);

            return policy;
        }

        private static void ApplyPolicyRequest(
            DeliveryFeePolicy policy,
            DeliveryFeePolicyRequest request,
            DateTime? updatedAt)
        {
            policy.Name = request.Name!.Trim();
            policy.BaseFee = RoundMoney(request.BaseFee);
            policy.MinFee = request.MinFee.HasValue ? RoundMoney(request.MinFee.Value) : null;
            policy.MaxFee = request.MaxFee.HasValue ? RoundMoney(request.MaxFee.Value) : null;
            policy.SmallOrderThreshold = request.SmallOrderThreshold.HasValue ? RoundMoney(request.SmallOrderThreshold.Value) : null;
            policy.SmallOrderSurcharge = RoundMoney(request.SmallOrderSurcharge);
            policy.RushHourSurcharge = RoundMoney(request.RushHourSurcharge);
            policy.IsActive = request.IsActive;

            if (updatedAt.HasValue)
                policy.UpdatedAt = updatedAt.Value;
        }

        private static List<DeliveryFeeDistanceTier> BuildDistanceTiers(
            Guid policyId,
            IEnumerable<DeliveryFeeDistanceTierRequest> tierRequests)
        {
            return tierRequests
                .OrderBy(tier => tier.FromKm)
                .Select(tier => new DeliveryFeeDistanceTier
                {
                    Id = Guid.NewGuid(),
                    PolicyId = policyId,
                    FromKm = RoundDistance(tier.FromKm),
                    ToKm = tier.ToKm.HasValue ? RoundDistance(tier.ToKm.Value) : null,
                    FeePerKm = RoundMoney(tier.FeePerKm)
                })
                .ToList();
        }

        private static DeliveryFeePolicyResponse MapPolicy(DeliveryFeePolicy policy)
        {
            return new DeliveryFeePolicyResponse
            {
                Id = policy.Id,
                Name = policy.Name,
                BaseFee = policy.BaseFee,
                MinFee = policy.MinFee,
                MaxFee = policy.MaxFee,
                SmallOrderThreshold = policy.SmallOrderThreshold,
                SmallOrderSurcharge = policy.SmallOrderSurcharge,
                RushHourSurcharge = policy.RushHourSurcharge,
                IsActive = policy.IsActive,
                CreatedAt = policy.CreatedAt,
                UpdatedAt = policy.UpdatedAt,
                DistanceTiers = policy.DistanceTiers
                    .OrderBy(tier => tier.FromKm)
                    .Select(tier => new DeliveryFeeDistanceTierResponse
                    {
                        Id = tier.Id,
                        FromKm = tier.FromKm,
                        ToKm = tier.ToKm,
                        FeePerKm = tier.FeePerKm
                    })
                    .ToList()
            };
        }

        private static List<string> ValidatePolicyRequest(DeliveryFeePolicyRequest? request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("Delivery fee policy request is required");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add("Name is required");
            else if (request.Name.Trim().Length > 100)
                errors.Add("Name must be 100 characters or less");

            if (request.BaseFee < 0m)
                errors.Add("BaseFee must be greater than or equal to 0");

            if (request.MinFee.HasValue && request.MinFee.Value < 0m)
                errors.Add("MinFee must be greater than or equal to 0");

            if (request.MaxFee.HasValue && request.MaxFee.Value < 0m)
                errors.Add("MaxFee must be greater than or equal to 0");

            if (request.MinFee.HasValue && request.MaxFee.HasValue && request.MinFee.Value > request.MaxFee.Value)
                errors.Add("MinFee must be less than or equal to MaxFee");

            if (request.SmallOrderThreshold.HasValue && request.SmallOrderThreshold.Value < 0m)
                errors.Add("SmallOrderThreshold must be greater than or equal to 0");

            if (request.SmallOrderSurcharge < 0m)
                errors.Add("SmallOrderSurcharge must be greater than or equal to 0");

            if (request.RushHourSurcharge < 0m)
                errors.Add("RushHourSurcharge must be greater than or equal to 0");

            ValidateDistanceTiers(request.DistanceTiers, errors);

            return errors;
        }

        private static void ValidateDistanceTiers(
            IReadOnlyCollection<DeliveryFeeDistanceTierRequest>? tiers,
            List<string> errors)
        {
            if (tiers == null || tiers.Count == 0)
            {
                errors.Add("At least one distance tier is required");
                return;
            }

            var orderedTiers = tiers.OrderBy(tier => tier.FromKm).ToList();
            decimal? previousToKm = null;

            for (var index = 0; index < orderedTiers.Count; index++)
            {
                var tier = orderedTiers[index];
                var prefix = $"DistanceTiers[{index}]";

                if (tier.FromKm < 0m)
                    errors.Add($"{prefix}.FromKm must be greater than or equal to 0");

                if (tier.ToKm.HasValue && tier.ToKm.Value <= tier.FromKm)
                    errors.Add($"{prefix}.ToKm must be greater than FromKm");

                if (tier.FeePerKm < 0m)
                    errors.Add($"{prefix}.FeePerKm must be greater than or equal to 0");

                if (previousToKm.HasValue && tier.FromKm < previousToKm.Value)
                    errors.Add($"{prefix} overlaps with the previous distance tier");

                if (!previousToKm.HasValue && index > 0)
                    errors.Add($"{prefix} cannot appear after an open-ended distance tier");

                previousToKm = tier.ToKm;
            }
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
