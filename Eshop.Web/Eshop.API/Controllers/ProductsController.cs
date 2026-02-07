using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Eshop.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Eshop.API.Services;

namespace Eshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 11,
            [FromQuery] string? q = null,
            [FromQuery] int? categoryId = null
        )
        {
            var result = await _productService.GetProductsAsync(page, pageSize, q, categoryId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductDto productDto)
        {
            var createdProduct = await _productService.CreateProductAsync(productDto);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, ProductDto productDto)
        {
            if (id != productDto.Id)
                return BadRequest();

            var success = await _productService.UpdateProductAsync(id, productDto);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetImage(int id)
        {
            var imageData = await _productService.GetProductImageAsync(id);
            if (imageData == null)
                return NotFound();

            return File(imageData.Value.ImageBytes, imageData.Value.ContentType);
        }

        [HttpPost("{id}/image")]
        [RequestSizeLimit(10_000_000)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only image files are allowed.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var success = await _productService.UpdateProductImageAsync(id, ms.ToArray(), file.ContentType, file.FileName, file.Length);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
