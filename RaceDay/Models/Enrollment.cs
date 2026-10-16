namespace RaceDay.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }

        public int EventId { get; set; }

        public int ParticipantId { get; set; }

        public int CategoryId { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Confirmed";


        public Event Event { get; set; } = null!;

        public User Participant { get; set; } = null!;

        public Category Category { get; set; } = null!;

        public Result? Result { get; set; }
    }
}