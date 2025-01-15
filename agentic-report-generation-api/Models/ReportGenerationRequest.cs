using System.ComponentModel.DataAnnotations;

namespace AgenticReportGenerationApi.Models
{
    public class ReportGenerationRequest
    {
        public string? SessionId { get; set; }

        public string? UserId { get; set; }

        public string? CompanyId { get; set; }

        public  string? CompanyName { get; set; }

        [Required]
        public required string Prompt { get; set; }
    }
}
