namespace Messaging.Contracts.Common
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public PaginationRequest PaginationRequest { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PaginationRequest.PageSize);

        public bool HasNextPage => PaginationRequest.PageIndex < TotalPages;
        public bool HasPreviousPage => PaginationRequest.PageIndex > 1;

        public PagedResult()
        {
            Items = new List<T>();
        }

        public PagedResult(IEnumerable<T> itemList)
        {
            Items = itemList;
        }
    }
}
