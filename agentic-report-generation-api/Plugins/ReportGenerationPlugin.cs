using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AgenticReportGenerationApi.Plugins
{
    public class ReportGenerationPlugin
    {
        // TODO: pass company in from cache
        private readonly IMemoryCache _memoryCache;

        public ReportGenerationPlugin(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [KernelFunction("get_executive_summary")]
        [Description("Generate an executive summary.")]
        public async Task GenerateExecutiveSummaryAsync()
        {

        }

        [KernelFunction("summarize_executive_board_changes")]
        [Description("Summarize executive or board changes.")]
        public async Task SummarizeExecutiveBoardChangesAsync()
        {

        }

        [KernelFunction("summarize_rra_activity")]
        [Description("Summarize RRA activity over the last three years.")]
        public async Task SummarizeRraActivityAsync()
        {

        }

        [KernelFunction("confirm_asn")]
        [Description("Confirm if ASN was conducted with the client in the last three years.")]
        public async Task ConfirmAsnAsync()
        {

        }

        [KernelFunction("summarize_financials")]
        [Description("Summarize financials for the client.")]
        public async Task SummarizeFinancialsAsync()
        {

        }

        [KernelFunction("summarize_corporate_timelines")]
        [Description("Summarize corporate timelines for the client.")]
        public async Task SummarizeCorporateTimelinesAsync()
        {

        }

        [KernelFunction("get_full_summary")]
        [Description("Get full summary.")]
        public async Task GetFullSummaryAsync()
        {

        }

        private async Task<Company> GetCompanyAsync(string companyName)
        {
            _memoryCache.TryGetValue(companyName, out Company company);
            return company;
        }
    }
}