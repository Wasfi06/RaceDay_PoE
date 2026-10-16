
namespace RaceDayMVC.Models
{
    public class ResultViewModel
    {
        public int ResultId { get; set; }
        public int EnrollmentId { get; set; }
        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        public TimeSpan? FinishTime { get; set; }
        public int? FinishPosition { get; set; }

        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}

