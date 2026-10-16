using static System.Collections.Specialized.BitVector32;

namespace RaceDay.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? ProfileImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;


        public ICollection<Session> Sessions { get; set; } = new List<Session>();

        public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}