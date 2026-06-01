using AddressService.DTOs;
using Messaging.Contracts.Common;

namespace AddressService.Services.Interfaces
{
    public interface IAddressService
    {
        Task<ApiResponse<PagedResult<ProvinceResponse>>> GetProvincesAsync(
            PaginationRequest paginationRequest,
            string? search,
            CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<ProvinceResponse>>> SearchProvincesByNameAsync(
            string key,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default);
        Task<ApiResponse<ProvinceResponse>> GetProvinceByCodeAsync(string provinceCode, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<WardResponse>>> GetWardsByProvinceCodeAsync(
            string provinceCode,
            PaginationRequest paginationRequest,
            string? search,
            CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<WardResponse>>> SearchWardsByNameAsync(
            string key,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default);
        Task<ApiResponse<WardResponse>> GetWardByCodeAsync(string wardCode, CancellationToken cancellationToken = default);
        Task<ApiResponse<ProvinceResponse>> GetProvinceByWardCodeAsync(string wardCode, CancellationToken cancellationToken = default);
        Task<ApiResponse<AddressResolutionResponse>> ResolveAddressAsync(
            AddressResolutionRequest request,
            CancellationToken cancellationToken = default);
    }
}
