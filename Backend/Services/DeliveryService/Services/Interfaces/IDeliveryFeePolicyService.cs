using DeliveryService.DTOs;
using Messaging.Contracts.Common;

namespace DeliveryService.Services.Interfaces
{
    public interface IDeliveryFeePolicyService
    {
        Task<ApiResponse<PagedResult<DeliveryFeePolicyResponse>>> GetPoliciesAsync(
            PaginationRequest paginationRequest,
            bool includeInactive,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<DeliveryFeePolicyResponse>> GetPolicyAsync(
            Guid policyId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<DeliveryFeePolicyResponse>> CreatePolicyAsync(
            DeliveryFeePolicyRequest? request,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<DeliveryFeePolicyResponse>> UpdatePolicyAsync(
            Guid policyId,
            DeliveryFeePolicyRequest? request,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<ConfirmationResponse>> DeletePolicyAsync(
            Guid policyId,
            CancellationToken cancellationToken = default);
    }
}
