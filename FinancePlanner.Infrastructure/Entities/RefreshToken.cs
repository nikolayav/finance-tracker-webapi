namespace FinancePlanner.Infrastructure.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;

        public bool IsValid => DateTime.UtcNow < Expiry;
    }
}
