namespace RaceDay.Models
{
    public class Session
    {
        public Guid SessionId { get; set; }

        public int UserId { get; set; }

        public string RoleSnapshot { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public User User { get; set; } = null!;
    }
}