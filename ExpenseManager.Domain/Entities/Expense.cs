using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseManager.Domain.Entities
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        public string CompanyId { get; set; } = string.Empty;

        public string? ReceiptPath { get; set; }

        public string? PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
