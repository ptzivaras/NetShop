namespace Eshop.Web.ViewModels
{
    public sealed class ProductsListViewModel
    {
        public List<ProductViewModel> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public string? Query { get; set; }
        public int? CategoryId { get; set; }
    }
}
