namespace RaceDay.DTOs
{
    public class UpdateEventRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? RouteUrl { get; set; }
        public string? RouteDescription { get; set; }
        public string? BannerImageUrl { get; set; }
    }
}
