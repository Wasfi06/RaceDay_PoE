using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CreateCategoryViewModel
    {
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string CategoryType { get; set; } = "Age";

        [Range(0, 120)]
        public int? MinAge { get; set; }

        [Range(0, 120)]
        public int? MaxAge { get; set; }

        [Range(0, 1000)]
        public decimal? MinDistanceKm { get; set; }

        [Range(0, 1000)]
        public decimal? MaxDistanceKm { get; set; }
    }
}
