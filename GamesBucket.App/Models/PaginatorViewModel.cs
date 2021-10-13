namespace GamesBucket.App.Models
{
    public class PaginatorViewModel
    {
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int ResultsCount { get; set; }
        public int RowCount { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}