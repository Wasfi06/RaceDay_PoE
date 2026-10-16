namespace RaceDay.DTOs
{
    public class CreateResultRequest
    {
        public int EnrollmentId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? FinishPosition { get; set; }
        public bool IsPublished { get; set; }
    }
}
