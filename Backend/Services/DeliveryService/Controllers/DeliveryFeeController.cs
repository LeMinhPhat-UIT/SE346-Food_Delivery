using DeliveryService.DTOs;
using DeliveryService.Services.Interfaces;
using Messaging.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Controllers
{
    [Route("api/delivery-fee")]
    [ApiController]
    [Authorize]
    public class DeliveryFeeController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;
        private readonly IDeliveryFeePolicyService _policyService;

        public DeliveryFeeController(
            IDeliveryService deliveryService,
            IDeliveryFeePolicyService policyService)
        {
            _deliveryService = deliveryService;
            _policyService = policyService;
        }

        [HttpPost("quote")]
        public async Task<ActionResult<ApiResponse<EstimateDeliveryFeeResponse>>> QuoteDeliveryFee([FromBody] EstimateDeliveryFeeRequest? request)
        {
            try
            {
                var response = await _deliveryService.EstimateDeliveryFeeAsync(request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("policies")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<PagedResult<DeliveryFeePolicyResponse>>>> GetPolicies(
            [FromQuery] PaginationRequest paginationRequest,
            [FromQuery] bool includeInactive = true)
        {
            try
            {
                var response = await _policyService.GetPoliciesAsync(paginationRequest, includeInactive);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("policies/{policyId:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<DeliveryFeePolicyResponse>>> GetPolicy([FromRoute] Guid policyId)
        {
            try
            {
                var response = await _policyService.GetPolicyAsync(policyId);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("policies")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<DeliveryFeePolicyResponse>>> CreatePolicy([FromBody] DeliveryFeePolicyRequest? request)
        {
            try
            {
                var response = await _policyService.CreatePolicyAsync(request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("policies/{policyId:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<DeliveryFeePolicyResponse>>> UpdatePolicy(
            [FromRoute] Guid policyId,
            [FromBody] DeliveryFeePolicyRequest? request)
        {
            try
            {
                var response = await _policyService.UpdatePolicyAsync(policyId, request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("policies/{policyId:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> DeletePolicy([FromRoute] Guid policyId)
        {
            try
            {
                var response = await _policyService.DeletePolicyAsync(policyId);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
