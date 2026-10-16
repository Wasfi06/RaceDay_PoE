
namespace RaceDayMVC.Models
{
    public class OrganizerEventResultsViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;

        public List<OrganizerResultRowViewModel> Participants { get; set; }
            = new List<OrganizerResultRowViewModel>();
    }

    public class OrganizerResultRowViewModel
    {
        public int EnrollmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        public int? ResultId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? FinishPosition { get; set; }
        public bool IsPublished { get; set; }
    }
}
