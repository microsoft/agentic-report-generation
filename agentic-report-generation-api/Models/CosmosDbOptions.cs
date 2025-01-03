using System.ComponentModel.DataAnnotations;

namespace AgenticReportGenerationApi.Models
{
    public class CosmosDbOptions
    {
        public const string CosmosDb = "CosmosDb";

        [Required]
        public string DatabaseName { get; set; }
        [Required]
        public string ContainerName { get; set; }
        [Required]
        public string AccountUri { get; set; }
        [Required]
        public string TenantId {  get; set; }
    }
}