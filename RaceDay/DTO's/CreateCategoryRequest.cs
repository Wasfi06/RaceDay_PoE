namespace RaceDay.DTOs
{
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public decimal? MinDistanceKm { get; set; }
        public decimal? MaxDistanceKm { get; set; }
    }
}