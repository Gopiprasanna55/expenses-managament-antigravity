namespace ExpenseManager.Application.DTOs
{
    public class WalletDto
    {
        public decimal TotalCreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal TotalSpent { get; set; }
        public string CompanyId { get; set; } = string.Empty;
    }

    public class WalletActivityDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Description { get; set; }
    }
}
