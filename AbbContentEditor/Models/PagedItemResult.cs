namespace AbbContentEditor.Models
{
    public class PagedItemResult<T>
    {
        public int TotalCount { get; set; }   // total items without paging
        public int PageNumber { get; set; }   // current page
        public int PageSize { get; set; }     // items per page
        public IQueryable<T> Query { get; set; }    // / paged query (still IQueryable)
    }

    public class PagedItemResultDto<T>
    {
        public int TotalCount { get; set; }  
        public int PageNumber { get; set; }
        public int PageSize { get; set; }   
        public List<T> Items{ get; set; } 

    }
}
