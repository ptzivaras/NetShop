using Eshop.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Eshop.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // [Timestamp] sets ValueGeneratedOnAddOrUpdate. Non-SQL-Server providers
            // (SQLite, InMemory) cannot auto-generate rowversion values, so EF Core
            // sends NULL and the column's NOT NULL constraint fails. Fix: use the
            // client-provided value directly on these providers.
            var provider = Database.ProviderName ?? "";
            if (!provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                modelBuilder.Entity<Product>()
                    .Property(p => p.RowVersion)
                    .ValueGeneratedNever();
            }
        }

public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
    }
}
