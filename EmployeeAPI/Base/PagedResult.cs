namespace EmployeeAPI.Base
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public bool HasNextPage  =>  PageIndex * PageSize < TotalCount;
        public bool HasPreviousPage => PageIndex > 1;
    }
}
