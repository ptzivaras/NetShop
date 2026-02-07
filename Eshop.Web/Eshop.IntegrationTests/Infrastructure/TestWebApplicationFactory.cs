using Eshop.Core.Data;
using Eshop.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Eshop.IntegrationTests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });
        });

        // Configure to skip database seeding (no migrations on in-memory database)
        builder.UseEnvironment("Testing");
    }

    // Seed test data after factory creation
    public void SeedTestData()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Seed test categories
        if (!db.Categories.Any())
        {
            db.Categories.AddRange(
                new Category { Id = 1, Name = "Electronics", Description = "Electronic devices" },
                new Category { Id = 2, Name = "Books", Description = "Physical and digital books" }
            );
        }

        // Seed test products
        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new Product { Id = 1, Name = "Laptop", Description = "Gaming Laptop", Price = 1200, StockQuantity = 5, CategoryId = 1 },
                new Product { Id = 2, Name = "Mouse", Description = "Wireless Mouse", Price = 25, StockQuantity = 50, CategoryId = 1 }
            );
        }

        db.SaveChanges();
    }
}
