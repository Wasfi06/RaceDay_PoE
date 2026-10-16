
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CreateEventViewModel
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EventDate { get; set; } = DateTime.Now.AddDays(7);

        [Required]
        public string Location { get; set; } = string.Empty;

        [Range(0.1, 1000)]
        public decimal DistanceKm { get; set; }

        [Required]
        public string EventType { get; set; } = string.Empty;

        [Url]
        public string? RouteUrl { get; set; }

        public string? RouteDescription { get; set; }

        [Url]
        public string? BannerImageUrl { get; set; }
    }
}

