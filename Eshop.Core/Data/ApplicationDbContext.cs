using Eshop.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Eshop.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<StockAlert> StockAlerts { get; set; }

}
//Add-Migration InitialCreate -Project Eshop.Core -StartupProject Eshop.Web
//Update-Database

//Add-Migration AddEshopSchema -Project Eshop.Core -StartupProject Eshop.Web -OutputDir Data/Migrations
//Update-Database -Project Eshop.Core -StartupProject Eshop.Web

