using AddressService.Entities;
using AddressService.Persistences;
using AddressService.Repositories.Interfaces;
using Messaging.Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace AddressService.Repositories.Implements
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AddressDbContext _context;

        public AddressRepository(AddressDbContext context)
        {
            _context = context;
        }

        public async Task<(IReadOnlyList<Province> Items, int TotalCount)> GetProvincesAsync(
            PaginationRequest paginationRequest,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Provinces
                .AsNoTracking()
                .Include(p => p.AdministrativeUnit)
                .AsQueryable();

            query = ApplyProvinceSearch(query, search);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(p => p.Code)
                .Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(IReadOnlyList<Province> Items, int TotalCount)> SearchProvincesByNameAsync(
            string key,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Provinces
                .AsNoTracking()
                .Include(p => p.AdministrativeUnit)
                .AsQueryable();

            query = ApplyProvinceNameSearch(query, key);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(p => p.Code)
                .Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<Province?> GetProvinceByCodeAsync(string provinceCode, CancellationToken cancellationToken = default)
        {
            var normalizedProvinceCode = provinceCode.Trim();

            return await _context.Provinces
                .AsNoTracking()
                .Include(p => p.AdministrativeUnit)
                .FirstOrDefaultAsync(p => p.Code == normalizedProvinceCode, cancellationToken);
        }

        public async Task<(IReadOnlyList<Ward> Items, int TotalCount)> GetWardsByProvinceCodeAsync(
            string provinceCode,
            PaginationRequest paginationRequest,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var normalizedProvinceCode = provinceCode.Trim();
            var query = _context.Wards
                .AsNoTracking()
                .Include(w => w.Province)
                .Include(w => w.AdministrativeUnit)
                .Where(w => w.ProvinceCode == normalizedProvinceCode);

            query = ApplyWardSearch(query, search);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(w => w.Code)
                .Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(IReadOnlyList<Ward> Items, int TotalCount)> SearchWardsByNameAsync(
            string key,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Wards
                .AsNoTracking()
                .Include(w => w.Province)
                .Include(w => w.AdministrativeUnit)
                .AsQueryable();

            query = ApplyWardNameSearch(query, key);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(w => w.ProvinceCode)
                .ThenBy(w => w.Code)
                .Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<Ward?> GetWardByCodeAsync(string wardCode, CancellationToken cancellationToken = default)
        {
            var normalizedWardCode = wardCode.Trim();

            return await _context.Wards
                .AsNoTracking()
                .Include(w => w.Province)
                    .ThenInclude(p => p!.AdministrativeUnit)
                .Include(w => w.AdministrativeUnit)
                .FirstOrDefaultAsync(w => w.Code == normalizedWardCode, cancellationToken);
        }

        public async Task<Province?> GetProvinceByWardCodeAsync(string wardCode, CancellationToken cancellationToken = default)
        {
            var normalizedWardCode = wardCode.Trim();

            return await _context.Provinces
                .AsNoTracking()
                .Include(p => p.AdministrativeUnit)
                .Where(p => _context.Wards.Any(w => w.Code == normalizedWardCode && w.ProvinceCode == p.Code))
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static IQueryable<Province> ApplyProvinceSearch(IQueryable<Province> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var pattern = $"%{search.Trim()}%";

            return query.Where(p =>
                EF.Functions.ILike(p.Code, pattern) ||
                EF.Functions.ILike(p.Name, pattern) ||
                EF.Functions.ILike(p.FullName, pattern) ||
                (p.NameEn != null && EF.Functions.ILike(p.NameEn, pattern)) ||
                (p.FullNameEn != null && EF.Functions.ILike(p.FullNameEn, pattern)) ||
                (p.CodeName != null && EF.Functions.ILike(p.CodeName, pattern)));
        }

        private static IQueryable<Province> ApplyProvinceNameSearch(IQueryable<Province> query, string key)
        {
            var pattern = $"%{key.Trim()}%";

            return query.Where(p =>
                EF.Functions.ILike(p.Name, pattern) ||
                EF.Functions.ILike(p.FullName, pattern) ||
                (p.NameEn != null && EF.Functions.ILike(p.NameEn, pattern)) ||
                (p.FullNameEn != null && EF.Functions.ILike(p.FullNameEn, pattern)));
        }

        private static IQueryable<Ward> ApplyWardSearch(IQueryable<Ward> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var pattern = $"%{search.Trim()}%";

            return query.Where(w =>
                EF.Functions.ILike(w.Code, pattern) ||
                EF.Functions.ILike(w.Name, pattern) ||
                (w.FullName != null && EF.Functions.ILike(w.FullName, pattern)) ||
                (w.NameEn != null && EF.Functions.ILike(w.NameEn, pattern)) ||
                (w.FullNameEn != null && EF.Functions.ILike(w.FullNameEn, pattern)) ||
                (w.CodeName != null && EF.Functions.ILike(w.CodeName, pattern)));
        }

        private static IQueryable<Ward> ApplyWardNameSearch(IQueryable<Ward> query, string key)
        {
            var pattern = $"%{key.Trim()}%";

            return query.Where(w =>
                EF.Functions.ILike(w.Name, pattern) ||
                (w.FullName != null && EF.Functions.ILike(w.FullName, pattern)) ||
                (w.NameEn != null && EF.Functions.ILike(w.NameEn, pattern)) ||
                (w.FullNameEn != null && EF.Functions.ILike(w.FullNameEn, pattern)));
        }
    }
}
