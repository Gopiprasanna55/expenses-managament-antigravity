using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseManager.Domain.Entities
{
    public class Wallet
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalCreditLimit { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string CompanyId { get; set; } = string.Empty;
    }
}
