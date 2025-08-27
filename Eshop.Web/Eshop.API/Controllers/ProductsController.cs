using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Eshop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Eshop.Core.Data;
using Eshop.Contracts.DTOs;

namespace Eshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 11,
            [FromQuery] string? q = null,
            [FromQuery] int? categoryId = null
        )
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

            var result = new PagedResult<ProductDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            return product;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest();

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                    return NotFound();

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("The product may have already been deleted by another user.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetImage(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || product.ImageBytes == null || string.IsNullOrEmpty(product.ImageContentType))
                return NotFound();

            return File(product.ImageBytes, product.ImageContentType);
        }

        [HttpPost("{id}/image")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only image files are allowed.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            product.ImageBytes = ms.ToArray();
            product.ImageContentType = file.ContentType;
            product.ImageFileName = file.FileName;
            product.ImageSize = file.Length;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
