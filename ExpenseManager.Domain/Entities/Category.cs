using System.ComponentModel.DataAnnotations;

namespace ExpenseManager.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string CompanyId { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
