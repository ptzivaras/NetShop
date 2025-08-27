using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eshop.Core.Data;
using Eshop.Contracts.DTOs;
using Eshop.Web.Data;

namespace Eshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProductsController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 11,
            [FromQuery] string? q = null,
            [FromQuery] int? categoryId = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 11;

            var query = _context.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Name.Contains(q) || p.Description.Contains(q));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var totalCount = await query.CountAsync();
            query = query.OrderBy(p => p.Id);
            //
            var items = await query
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .Select(c => new ProductDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    StockQuantity = c.StockQuantity,
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category != null ? c.Category.Name : null
                }).ToListAsync();

            return Ok(new PagedResult<ProductDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
    }
}
