using Messaging.Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace Messaging.Contracts.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default) where T : class
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (paginationRequest.PageIndex <= 0) paginationRequest.PageIndex = 1;
            if (paginationRequest.PageSize <= 0) paginationRequest.PageSize = 10;

            paginationRequest.PageSize = Math.Min(paginationRequest.PageSize, 100);

            var totalCount = await query.CountAsync(cancellationToken);

            var skip = (paginationRequest.PageIndex - 1) * paginationRequest.PageSize;

            var items = await query
                .AsNoTracking()
                .Skip(skip)
                .Take(paginationRequest.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>
            {
                Items = items,
                PaginationRequest = paginationRequest,
                TotalCount = totalCount
            };
        }
    }
}
