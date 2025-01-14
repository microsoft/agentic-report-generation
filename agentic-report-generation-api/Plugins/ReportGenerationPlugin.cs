using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

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
        public string GenerateCompanyOverview(
            [Description("The company id of the company for which to generate a company overview.")] string companyId,
            [Description("The company name for which to generate a company overview.")] string companyName)
        {
            _logger.LogInformation($"Generating overview summary for company '{companyName} ({companyId})'.");
            var result = string.Empty;

            var company = GetCompany(companyId);

            if (company != null)
            {
                result = company.company_description;

                _logger.LogInformation($"End generating overview summary for company '{companyName}'.");
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
        public string SummarizeBoardChanges(
            [Description("The company id of the company for which to generate a summary of board changes.")] string companyId,
            [Description("The company name for which to generate a summary of board changes.")] string companyName)
        {
            _logger.LogInformation($"Generating board changes for company '{companyName} ({companyId})'.");
            var result = string.Empty;
            var company = GetCompany(companyId);

            if (company != null)
            {
                var boardMembers = company.board_members.ToList();
                result = string.Join(Environment.NewLine, boardMembers.Select(dict => JsonSerializer.Serialize(dict)));
                _logger.LogInformation($"End generating board changes for company '{companyName}'.");
            }
            else
            {
                result = $"Company '{companyName}' not found.";
                _logger.LogWarning(result);
            }

            return result;
        }

        [KernelFunction("summarize_executive_changes")]
        [Description("Summarize executive changes for a given company.")]
        public string SummarizeExecutiveChanges(
            [Description("The company id of the company for which to generate a summary of executive changes.")] string companyId,
            [Description("The company name for which to generate a summary of executive changes.")] string companyName)
        {
            _logger.LogInformation($"Generating executive changes for company '{companyName} ({companyId})'.");
            var result = string.Empty;
            var company = GetCompany(companyId);

            if (company != null)
            {
                var executiveChanges = company.top_executives.ToList();
                result = string.Join(Environment.NewLine, executiveChanges.Select(dict => JsonSerializer.Serialize(dict)));
                _logger.LogInformation($"End generating executive changes for company '{companyName}'.");
            }
            else
            {
                result = $"Company '{companyName}' not found.";
                _logger.LogWarning(result);
            }

            return result;
        }

        [KernelFunction("summarize_rra_activity")]
        [Description("Summarize RRA activity for a specified fiscal year or range of fiscal years when explicitly asked for by the user.")]
        public string SummarizeRraActivity(
            [Description("The company id of the company for which to generate a summary of RRA activity.")] string companyId,
            [Description("The company name for which to generate a summary of RRA activity.")] string companyName)
        {
            _logger.LogInformation($"Generating RRA summary for company '{companyName} ({companyId})'.");
            var result = string.Empty;
            var company = GetCompany(companyId);

            if (company != null)
            {
                var rraActivity = company.rra_activity.ToList();
                result = string.Join(Environment.NewLine, rraActivity.Select(dict => JsonSerializer.Serialize(dict)));
                _logger.LogInformation($"End generating RRA summary for company '{companyName}'.");
            }
            else
            {
                result = $"Company '{companyName}' not found.";
                _logger.LogWarning(result);
            }

            return result;
        }

        [KernelFunction("get_asn")]
        [Description("Gets the ASN, or new assignments, which were conducted for the company during the specified fiscal year or range of fiscal years when explicitly asked by the user.")]
        public string GetAsn(
            [Description("The company id of the company for which to generate ASN, or new assignments.")] string companyId,
            [Description("The company name for which to generate ASN, or new assignments.")] string companyName)
        {
            _logger.LogInformation($"Generating ASN summary for company '{companyName} ({companyId})'.");
            var result = string.Empty;
            var company = GetCompany(companyId);

            if (company != null)
            {
                List<Dictionary<string, object>> newAssignments = company.rra_activity.Where(a => a.category.Equals("New Assignments", StringComparison.OrdinalIgnoreCase))
                                                         .Select(a => a.DynamicFields)
                                                         .ToList();

                result = string.Join(Environment.NewLine, newAssignments.Select(dict => JsonSerializer.Serialize(dict)));
            }
            else
            {
                result = $"Company '{companyName}' not found.";
                _logger.LogWarning(result);
            }

            return result;
        }

        [KernelFunction("summarize_financials")]
        [Description("Summarize financial data for a given company.")]
        public string SummarizeFinancials(
            [Description("The company id of the company for which to generate summary of financial data.")] string companyId,
            [Description("The company name for which to generate a summary of financial data.")] string companyName)
        {
            _logger.LogInformation($"Generating financial summary for company '{companyName} ({companyId})'.");
            var result = string.Empty;
            var company = GetCompany(companyId);

            if (company != null)
            {
                FinancialData[] financialDataArray = company.financial_data.ToArray();
                result = string.Join(Environment.NewLine, financialDataArray.Select(fd => fd.ToString()));
                _logger.LogInformation($"End generating financial summary for company '{companyName}'.");
            }
            else
            {
                result = $"Company '{companyName}' not found.";
                _logger.LogWarning(result);
            }

            return result;
        }

        [KernelFunction("summarize_corporate_timelines")]
        [Description("Summarize corporate timelines for a given company when explicitly asked for by the user.")]
        public string SummarizeCorporateTimelines(
            [Description("The company id of the company for which to generate the corporate timeline summary.")] string companyId,
            [Description("The company name for which to generate the corporate timeline summary.")] string companyName)
        {
            _logger.LogInformation($"Generating corporate timelines for company '{companyName} ({companyId})'.");
            var result = string.Empty;
            var company = GetCompany(companyId);

            if (company != null)
            {
                CorporateTimeline[] corporateTimelineArray = company.corporate_timelines.ToArray();
                result = string.Join(Environment.NewLine, corporateTimelineArray.Select(ct => ct.ToString()));
                _logger.LogInformation($"End generating corporate timelines for company '{companyName}'.");
            }
            else
            {
                result = $"Company '{companyName}' not found.";
                _logger.LogWarning(result);
            }

            return result;
        }

        [KernelFunction("get_full_summary")]
        [Description("Get full summary for a given company.")]
        public string GetFullSummary(
            [Description("The company id of the company for which to generate the summary.")] string companyId,
            [Description("The company name for which to generate the summary.")] string companyName)
        {
            throw new NotImplementedException();
        }

        [KernelFunction("get_news_summary")]
        [Description("Get news summary for a given company.")]
        public string GetNewsSummary(
            [Description("The company id of the company for which to generate the news summary.")] string companyId,
            [Description("The company name for which to generate the news summary.")] string companyName)
        {
            _logger.LogInformation($"Generating news summary for company '{companyName} ({companyId})'.");
            var result = string.Empty;
            var company = GetCompany(companyId);

            if (company != null)
            {
                NewsData[] newsDataArray = company.news_data.ToArray();
                result = string.Join(Environment.NewLine + Environment.NewLine, newsDataArray.Select(n =>
                $"NewsData:\n  Headline: {n.Headline}\n  Source: {n.Source}"));
                _logger.LogInformation($"End generating news summary for company '{companyName}'.");
            }
            else
            {
                result = $"Company '{companyName}' not found.";
                _logger.LogWarning(result);
            }

            return result;
        }

        private Company? GetCompany(string companyId)
        {
            _memoryCache.TryGetValue(companyId, out Company? company);
            return company;
        }
    }
}