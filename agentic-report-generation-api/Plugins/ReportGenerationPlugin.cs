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
        [Description("Generates an overview for a given company.")]
        public string GenerateCompanyOverview([Description("The name of the company for which to generate the summary.")] string companyName)
        {

            _logger.LogInformation($"Generating overview summary for company '{companyName}'.");
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

        [KernelFunction("summarize_board_changes")]
        [Description("Summarize board changes for a given company.")]
        public async Task SummarizeBoardChanges([Description("The name of the company for which to generate the summary.")] string companyName)
        {
        }

        [KernelFunction("summarize_executive_changes")]
        [Description("Summarize executive changes for a given company.")]
        public async Task SummarizeExecutiveChanges([Description("The name of the company for which to generate the summary.")] string companyName)
        {
        }

        [KernelFunction("summarize_rra_activity")]
        [Description("Summarize RRA activity for a given time range when explicitly asked for by the user.")]
        public async Task SummarizeRraActivity([Description("The name of the company for which to generate the summary.")] string companyName)
        {
        }

        [KernelFunction("confirm_asn")]
        [Description("Confirm if ASN was conducted with the client in the last three years for a given company.")]
        public async Task ConfirmAsn([Description("The name of the company for which to generate the summary.")] string companyName)
        {
        }

        [KernelFunction("summarize_financials")]
        [Description("Summarize financial data for a given company.")]
        public string SummarizeFinancials([Description("The name of the company for which to generate the summary.")] string companyName)
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
        public async Task SummarizeCorporateTimelines([Description("The name of the company for which to generate the summary.")] string companyName)
        {
        }

        [KernelFunction("get_full_summary")]
        [Description("Get full summary for a given company.")]
        public async Task GetFullSummary([Description("The name of the company for which to generate the summary.")] string companyName)
        {
        }

        [KernelFunction("get_news_summary")]
        [Description("Get news summary for a given company.")]
        public string GetNewsSummary([Description("The name of the company for which to generate the summary.")] string companyName)
        {
            _logger.LogInformation($"Generating news summary for company '{companyName}'.");
            var result = string.Empty;
            var company = GetCompany(companyName);

            if (company != null)
            {
                NewsData[] newsDataArray = company.news_data.ToArray();
                result = string.Join(Environment.NewLine + Environment.NewLine, newsDataArray.Select(n =>
                $"NewsData:\n  Headline: {n.Headline}\n  Source: {n.Source}"));
            }
            else
            {
                result = $"Company '{companyName}' not found.";
                _logger.LogWarning(result);
            }

            return result;
        }

        private Company? GetCompany(string companyName)
        {
            _memoryCache.TryGetValue(companyName, out Company? company);
            return company;
        }
    }
}