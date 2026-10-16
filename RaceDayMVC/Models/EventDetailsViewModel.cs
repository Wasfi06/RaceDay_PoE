namespace RaceDayMVC.Models
{
    public class EventDetailsViewModel
    {
        public EventViewModel Event { get; set; } = new();

        public List<CategoryViewModel> Categories { get; set; } = new();
    }
}