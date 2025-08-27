using System.Linq;
using Eshop.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Eshop.Core.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            context.Database.Migrate();

            // Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Electronics", Description = "Electronic items" },
                    new Category { Name = "Books", Description = "All kinds of books" },
                    new Category { Name = "Clothing", Description = "Apparel and garments" }
                );
                context.SaveChanges();
            }

            // Seed Products
            if (!context.Products.Any())
            {
                var electronicsCategory = context.Categories.First(c => c.Name == "Electronics");

                context.Products.AddRange(
                    new Product
                    {
                        Name = "Smartphone",
                        Description = "A high-end smartphone",
                        Price = 699.99m,
                        StockQuantity = 50,
                        CategoryId = electronicsCategory.Id
                    },
                    new Product
                    {
                        Name = "Headphones",
                        Description = "Noise-cancelling headphones",
                        Price = 199.99m,
                        StockQuantity = 20,
                        CategoryId = electronicsCategory.Id
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
