using Microsoft.AspNetCore.Mvc;
using Eshop.Web.Services.Interfaces;
using Eshop.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Eshop.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ICartService _cartService;
        private readonly UserManager<IdentityUser> _userManager;

        public ProductsController(IProductService productService, ICategoryService categoryService, ICartService cartService, UserManager<IdentityUser> userManager)
        {
            _productService = productService;
            _categoryService = categoryService;
            _cartService = cartService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 11, string? q = null, int? categoryId = null)
        {
            var products = await _productService.GetProductsPagedAsync(page, pageSize, q, categoryId);
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new ProductViewModel
            {
                Categories = await _categoryService.GetAllCategoriesAsync()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductViewModel product)
        {
            if (!ModelState.IsValid) return View(product);
            await _productService.CreateAsync(product);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(ProductViewModel product)
        {
            if (!ModelState.IsValid) return View(product);
            await _productService.UpdateAsync(product);
            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                await _productService.UploadImageAsync(product.Id, product.ImageFile);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
