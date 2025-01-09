using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AgenticReportGenerationApi.Plugins
{
    public class ReportGenerationPlugin
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public ReportGenerationPlugin(
            IMemoryCache memoryCache,
            ILogger logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        [KernelFunction("get_executive_summary")]
        [Description("Generates an executive summary for a given company.")]
        public string GenerateExecutiveSummaryAsync([Description("The name of the company for which to generate the summary. If a period is included, it may be part of the company name.")] string companyName)
        {
            _logger.LogInformation($"Generating executive summary for company '{companyName}'.");
            var result = string.Empty;

            var company = GetCompany(companyName);

            if (company != null)
            {
                result = company.company_description;
            }
            else
            {
                result = $"Company '{companyName}' not found.";
                _logger.LogWarning(result);
            }

            return result;
        }

        [KernelFunction("summarize_executive_board_changes")]
        [Description("Summarize executive and board changes for a given company.")]
        public async Task SummarizeExecutiveBoardChangesAsync([Description("The name of the company for which to generate the summary")] string companyName)
        {
        }

        [KernelFunction("summarize_rra_activity")]
        [Description("Summarize RRA activity over the last three years when explicitly asked for by the user.")]
        public async Task SummarizeRraActivityAsync([Description("The name of the company for which to generate the summary")] string companyName)
        {
        }

        [KernelFunction("confirm_asn")]
        [Description("Confirm if ASN was conducted with the client in the last three years for a given company.")]
        public async Task ConfirmAsnAsync([Description("The name of the company for which to generate the summary")] string companyName)
        {
        }

        [KernelFunction("summarize_financials")]
        [Description("Summarize financial data for a given company.")]
        public string SummarizeFinancialsAsync([Description("The name of the company for which to generate the summary")] string companyName)
        {
            _logger.LogInformation($"Generating financial summary for company '{companyName}'.");
            var result = string.Empty;
            var company = GetCompany(companyName);

            if (company != null)
            {
                FinancialData[] financialDataArray = company.financial_data.ToArray();
                result = string.Join(Environment.NewLine, financialDataArray.Select(fd => fd.ToString()));
            }
            else
            {
                result = $"Company '{companyName}' not found.";
                _logger.LogWarning(result);
            }

            return result;
        }

        [KernelFunction("summarize_corporate_timelines")]
        [Description("Summarize corporate timelines for the client when explicitly asked for by the user.")]
        public async Task SummarizeCorporateTimelinesAsync([Description("The name of the company for which to generate the summary")] string companyName)
        {
        }

        [KernelFunction("get_full_summary")]
        [Description("Get full summary.")]
        public async Task GetFullSummaryAsync([Description("The name of the company for which to generate the summary")] string companyName)
        {
        }

        private Company? GetCompany(string companyName)
        {
            _memoryCache.TryGetValue(companyName, out Company? company);
            return company;
        }
    }
}