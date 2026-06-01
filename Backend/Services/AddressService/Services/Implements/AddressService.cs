using AddressService.DTOs;
using AddressService.Mappers;
using AddressService.Repositories.Interfaces;
using AddressService.Services.Interfaces;
using Messaging.Contracts.Common;

namespace AddressService.Services.Implements
{
    public class AddressService : IAddressService
    {
        private const int MaxPageSize = 100;
        private readonly IAddressRepository _addressRepository;
        private readonly AddressMapper _mapper;

        public AddressService(IAddressRepository addressRepository, AddressMapper mapper)
        {
            _addressRepository = addressRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedResult<ProvinceResponse>>> GetProvincesAsync(
            PaginationRequest paginationRequest,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var normalizedPagination = NormalizePagination(paginationRequest);
            var (provinces, totalCount) = await _addressRepository.GetProvincesAsync(
                normalizedPagination,
                search,
                cancellationToken);

            var response = _mapper.ToProvinceResponses(provinces);
            var result = new PagedResult<ProvinceResponse>(response, normalizedPagination, totalCount);

            return new ApiResponse<PagedResult<ProvinceResponse>>(StatusCodes.Status200OK, result);
        }

        public async Task<ApiResponse<PagedResult<ProvinceResponse>>> SearchProvincesByNameAsync(
            string key,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return new ApiResponse<PagedResult<ProvinceResponse>>(StatusCodes.Status400BadRequest, "Search key is required");

            var normalizedPagination = NormalizePagination(paginationRequest);
            var (provinces, totalCount) = await _addressRepository.SearchProvincesByNameAsync(
                key,
                normalizedPagination,
                cancellationToken);

            var response = _mapper.ToProvinceResponses(provinces);
            var result = new PagedResult<ProvinceResponse>(response, normalizedPagination, totalCount);

            return new ApiResponse<PagedResult<ProvinceResponse>>(StatusCodes.Status200OK, result);
        }

        public async Task<ApiResponse<ProvinceResponse>> GetProvinceByCodeAsync(string provinceCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(provinceCode))
                return new ApiResponse<ProvinceResponse>(StatusCodes.Status400BadRequest, "Province code is required");

            var province = await _addressRepository.GetProvinceByCodeAsync(provinceCode, cancellationToken);
            if (province is null)
                return new ApiResponse<ProvinceResponse>(StatusCodes.Status404NotFound, "No province found");

            return new ApiResponse<ProvinceResponse>(StatusCodes.Status200OK, _mapper.ToProvinceResponse(province));
        }

        public async Task<ApiResponse<PagedResult<WardResponse>>> GetWardsByProvinceCodeAsync(
            string provinceCode,
            PaginationRequest paginationRequest,
            string? search,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(provinceCode))
                return new ApiResponse<PagedResult<WardResponse>>(StatusCodes.Status400BadRequest, "Province code is required");

            var province = await _addressRepository.GetProvinceByCodeAsync(provinceCode, cancellationToken);
            if (province is null)
                return new ApiResponse<PagedResult<WardResponse>>(StatusCodes.Status404NotFound, "No province found");

            var normalizedPagination = NormalizePagination(paginationRequest);
            var (wards, totalCount) = await _addressRepository.GetWardsByProvinceCodeAsync(
                provinceCode,
                normalizedPagination,
                search,
                cancellationToken);

            var response = _mapper.ToWardResponses(wards);
            var result = new PagedResult<WardResponse>(response, normalizedPagination, totalCount);

            return new ApiResponse<PagedResult<WardResponse>>(StatusCodes.Status200OK, result);
        }

        public async Task<ApiResponse<PagedResult<WardResponse>>> SearchWardsByNameAsync(
            string key,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return new ApiResponse<PagedResult<WardResponse>>(StatusCodes.Status400BadRequest, "Search key is required");

            var normalizedPagination = NormalizePagination(paginationRequest);
            var (wards, totalCount) = await _addressRepository.SearchWardsByNameAsync(
                key,
                normalizedPagination,
                cancellationToken);

            var response = _mapper.ToWardResponses(wards);
            var result = new PagedResult<WardResponse>(response, normalizedPagination, totalCount);

            return new ApiResponse<PagedResult<WardResponse>>(StatusCodes.Status200OK, result);
        }

        public async Task<ApiResponse<WardResponse>> GetWardByCodeAsync(string wardCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(wardCode))
                return new ApiResponse<WardResponse>(StatusCodes.Status400BadRequest, "Ward code is required");

            var ward = await _addressRepository.GetWardByCodeAsync(wardCode, cancellationToken);
            if (ward is null)
                return new ApiResponse<WardResponse>(StatusCodes.Status404NotFound, "No ward found");

            return new ApiResponse<WardResponse>(StatusCodes.Status200OK, _mapper.ToWardResponse(ward));
        }

        public async Task<ApiResponse<ProvinceResponse>> GetProvinceByWardCodeAsync(string wardCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(wardCode))
                return new ApiResponse<ProvinceResponse>(StatusCodes.Status400BadRequest, "Ward code is required");

            var province = await _addressRepository.GetProvinceByWardCodeAsync(wardCode, cancellationToken);
            if (province is null)
                return new ApiResponse<ProvinceResponse>(StatusCodes.Status404NotFound, "No province found for ward");

            return new ApiResponse<ProvinceResponse>(StatusCodes.Status200OK, _mapper.ToProvinceResponse(province));
        }

        public async Task<ApiResponse<AddressResolutionResponse>> ResolveAddressAsync(
            AddressResolutionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.ProvinceCode))
                return new ApiResponse<AddressResolutionResponse>(StatusCodes.Status400BadRequest, "Province code is required");

            if (string.IsNullOrWhiteSpace(request.WardCode))
                return new ApiResponse<AddressResolutionResponse>(StatusCodes.Status400BadRequest, "Ward code is required");

            var province = await _addressRepository.GetProvinceByCodeAsync(request.ProvinceCode, cancellationToken);
            if (province is null)
                return new ApiResponse<AddressResolutionResponse>(StatusCodes.Status404NotFound, "No province found");

            var ward = await _addressRepository.GetWardByCodeAsync(request.WardCode, cancellationToken);
            if (ward is null)
                return new ApiResponse<AddressResolutionResponse>(StatusCodes.Status404NotFound, "No ward found");

            if (!string.Equals(ward.ProvinceCode, province.Code, StringComparison.OrdinalIgnoreCase))
                return new ApiResponse<AddressResolutionResponse>(StatusCodes.Status400BadRequest, "Ward does not belong to the selected province");

            var response = _mapper.ToAddressResolutionResponse(province, ward, request.AddressLine);

            return new ApiResponse<AddressResolutionResponse>(StatusCodes.Status200OK, response);
        }

        private static PaginationRequest NormalizePagination(PaginationRequest paginationRequest)
        {
            return new PaginationRequest
            {
                PageIndex = Math.Max(1, paginationRequest.PageIndex),
                PageSize = Math.Clamp(paginationRequest.PageSize, 1, MaxPageSize)
            };
        }
    }
}
