namespace Eshop.Web.ViewModels
{
    public class ShoppingCartViewModel
    {
        public int Id { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new();

        public int TotalItems => Items.Sum(i => i.Quantity);
        public decimal TotalCost => Items.Sum(item => item.Subtotal);
    }
}
