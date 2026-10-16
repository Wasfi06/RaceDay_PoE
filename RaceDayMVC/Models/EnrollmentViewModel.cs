namespace RaceDayMVC.Models
{
    public class EnrollmentViewModel
    {
        public int EventId { get; set; }

        public int CategoryId { get; set; }
    }

    public class EnrollmentResponse
    {
        public string Message { get; set; } = string.Empty;

        public int EnrollmentId { get; set; }
    }
}