using FinancePlanner.Common.Enums;

namespace FinancePlanner.Infrastructure.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime OccurredOn { get; set; }
        public DateTime CreatedAt { get; set; }
        public Account Account { get; set; } = null!;
    }
}
