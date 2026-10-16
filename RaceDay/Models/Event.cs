namespace RaceDay.Models
{
    public class Event
    {
        public int EventId { get; set; }

        public int OrganizerId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }

        public string EventType { get; set; } = string.Empty;

        public string? RouteUrl { get; set; }

        public string? RouteDescription { get; set; }

        public string? BannerImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        public User Organizer { get; set; } = null!;

        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}