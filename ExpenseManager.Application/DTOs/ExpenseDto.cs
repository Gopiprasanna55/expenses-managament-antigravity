using System.ComponentModel.DataAnnotations;

namespace ExpenseManager.Application.DTOs
{
    public class ExpenseDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please enter an amount")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal? Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? NewCategoryName { get; set; }

        public string? UserId { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? VendorName { get; set; }
        
        public string? ReceiptPath { get; set; }

        public string? PaymentMethod { get; set; }

        public Microsoft.AspNetCore.Http.IFormFile? ReceiptFile { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
