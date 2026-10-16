namespace RaceDay.Models
{
    public class Result
    {
        public int ResultId { get; set; }

        public int EnrollmentId { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? FinishPosition { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedAt { get; set; }


        public Enrollment Enrollment { get; set; } = null!;
    }
}