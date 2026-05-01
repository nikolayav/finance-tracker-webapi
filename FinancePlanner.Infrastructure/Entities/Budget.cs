namespace FinancePlanner.Infrastructure.Entities
{
    public class Budget
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal LimitAmount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
    }
}
