namespace SemesterTwo.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public string? Message { get; set; } // Add the missing Message property
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}