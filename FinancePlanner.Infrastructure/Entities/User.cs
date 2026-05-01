namespace FinancePlanner.Infrastructure.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
