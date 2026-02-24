using System.ComponentModel.DataAnnotations;

namespace ExpenseManager.Application.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
