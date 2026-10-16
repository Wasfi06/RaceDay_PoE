namespace RaceDayMVC.Models
{
    public class OrganizerEnrollmentViewModel
    {
        public int EnrollmentId { get; set; }

        public int ParticipantId { get; set; }

        public string ParticipantName { get; set; } = string.Empty;

        public string ParticipantEmail { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public DateTime EnrollmentDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
