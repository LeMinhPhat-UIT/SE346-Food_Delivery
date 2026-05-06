namespace Messaging.Contracts.Common
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public PaginationRequest PaginationRequest { get; set; } = new PaginationRequest();
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PaginationRequest.PageSize);

        public bool HasNextPage => PaginationRequest.PageIndex < TotalPages;
        public bool HasPreviousPage => PaginationRequest.PageIndex > 1;

        public PagedResult()
        {
        }

        public PagedResult(IEnumerable<T> itemList)
        {
            Items = itemList;
        }
    }
}
