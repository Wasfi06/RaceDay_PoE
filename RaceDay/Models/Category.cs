namespace RaceDay.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string CategoryType { get; set; } = string.Empty;

        public int? MinAge { get; set; }

        public int? MaxAge { get; set; }

        public decimal? MinDistanceKm { get; set; }

        public decimal? MaxDistanceKm { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public Event Event { get; set; } = null!;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}