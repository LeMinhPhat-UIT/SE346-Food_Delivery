using AddressService.Entities;
using Messaging.Contracts.Common;

namespace AddressService.Repositories.Interfaces
{
    public interface IAddressRepository
    {
        Task<(IReadOnlyList<Province> Items, int TotalCount)> GetProvincesAsync(
            PaginationRequest paginationRequest,
            string? search,
            CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Province> Items, int TotalCount)> SearchProvincesByNameAsync(
            string key,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default);
        Task<Province?> GetProvinceByCodeAsync(string provinceCode, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Ward> Items, int TotalCount)> GetWardsByProvinceCodeAsync(
            string provinceCode,
            PaginationRequest paginationRequest,
            string? search,
            CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Ward> Items, int TotalCount)> SearchWardsByNameAsync(
            string key,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default);
        Task<Ward?> GetWardByCodeAsync(string wardCode, CancellationToken cancellationToken = default);
        Task<Province?> GetProvinceByWardCodeAsync(string wardCode, CancellationToken cancellationToken = default);
    }
}
