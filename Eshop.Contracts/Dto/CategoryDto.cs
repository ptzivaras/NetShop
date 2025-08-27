using System.ComponentModel.DataAnnotations;

namespace Eshop.Contracts.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
