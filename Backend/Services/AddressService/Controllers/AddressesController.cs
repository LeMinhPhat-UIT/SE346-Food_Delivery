using AddressService.DTOs;
using AddressService.Services.Interfaces;
using Messaging.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace AddressService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet("provinces")]
        public async Task<ActionResult<ApiResponse<PagedResult<ProvinceResponse>>>> GetProvinces(
            [FromQuery] PaginationRequest paginationRequest,
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _addressService.GetProvincesAsync(paginationRequest, search, cancellationToken);
                return ToActionResult(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("provinces/search")]
        public async Task<ActionResult<ApiResponse<PagedResult<ProvinceResponse>>>> SearchProvinces(
            [FromQuery] string key,
            [FromQuery] PaginationRequest paginationRequest,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _addressService.SearchProvincesByNameAsync(key, paginationRequest, cancellationToken);
                return ToActionResult(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("provinces/{provinceCode}")]
        public async Task<ActionResult<ApiResponse<ProvinceResponse>>> GetProvince(
            [FromRoute] string provinceCode,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _addressService.GetProvinceByCodeAsync(provinceCode, cancellationToken);
                return ToActionResult(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("provinces/{provinceCode}/wards")]
        public async Task<ActionResult<ApiResponse<PagedResult<WardResponse>>>> GetWardsByProvince(
            [FromRoute] string provinceCode,
            [FromQuery] PaginationRequest paginationRequest,
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _addressService.GetWardsByProvinceCodeAsync(
                    provinceCode,
                    paginationRequest,
                    search,
                    cancellationToken);

                return ToActionResult(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("wards/search")]
        public async Task<ActionResult<ApiResponse<PagedResult<WardResponse>>>> SearchWards(
            [FromQuery] string key,
            [FromQuery] PaginationRequest paginationRequest,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _addressService.SearchWardsByNameAsync(key, paginationRequest, cancellationToken);
                return ToActionResult(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("wards/{wardCode}")]
        public async Task<ActionResult<ApiResponse<WardResponse>>> GetWard(
            [FromRoute] string wardCode,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _addressService.GetWardByCodeAsync(wardCode, cancellationToken);
                return ToActionResult(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("wards/{wardCode}/province")]
        public async Task<ActionResult<ApiResponse<ProvinceResponse>>> GetProvinceByWard(
            [FromRoute] string wardCode,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _addressService.GetProvinceByWardCodeAsync(wardCode, cancellationToken);
                return ToActionResult(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("resolve")]
        public async Task<ActionResult<ApiResponse<AddressResolutionResponse>>> ResolveAddress(
            [FromBody] AddressResolutionRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _addressService.ResolveAddressAsync(request, cancellationToken);
                return ToActionResult(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private ActionResult<ApiResponse<T>> ToActionResult<T>(ApiResponse<T> response)
        {
            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }
    }
}
