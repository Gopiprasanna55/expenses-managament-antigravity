using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ExpenseManager.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        public string? CompanyId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
