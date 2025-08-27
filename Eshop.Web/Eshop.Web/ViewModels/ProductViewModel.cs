﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // add this

namespace Eshop.Web.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [Range(0, 1000)]
        public int StockQuantity { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public List<CategoryViewModel>? Categories { get; set; } = new();

        public int InCartQuantity { get; set; }

        public IFormFile? ImageFile { get; set; }
        public bool HasImage => ImageFile != null;
    }
}
