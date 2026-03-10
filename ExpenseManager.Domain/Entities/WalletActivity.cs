using ExpenseManager.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseManager.Domain.Entities
{
    public class WalletActivity
    {
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public WalletActivityType Type { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string CompanyId { get; set; } = string.Empty;
        
        public int? ExpenseId { get; set; }

        [ForeignKey("ExpenseId")]
        public virtual Expense? Expense { get; set; }

        public string? Description { get; set; }
    }
}
