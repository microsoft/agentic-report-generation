using Newtonsoft.Json;

public class Company
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }
    [JsonProperty(PropertyName = "CompanyName")]
    public string company_name { get; set; }
    public string thumbnail { get; set; }
    public string company_description { get; set; }
    public string revenue { get; set; }
    public string marketCap { get; set; }
    public List<BoardMember> board_members { get; set; }
    public List<TopExecutive> top_executives { get; set; }
    public List<CorporateTimeline> corporate_timelines { get; set; }
    public List<FinancialData> financial_data { get; set; }
    public List<NewsData> news_data { get; set; }
    public List<SummaryData> summary_data { get; set; }
}

public class BoardMember
{
    public string Name { get; set; }
    public string Role { get; set; }
    public int? Age { get; set; }
    public int? Tenure { get; set; }
}

public class CorporateTimeline
{
    public string Date { get; set; }
    public string Type { get; set; }
    public string Headline { get; set; }
}

public class FinancialData
{
    public string FiscalPeriodEnding { get; set; }
    public string Currency { get; set; }
    public double? TotalRevenue { get; set; }
    public double? NetIncome { get; set; }
    public double? NetIncomeMarginPercent { get; set; }

    public override string ToString()
    {
        return $"Fiscal Period Ending: {FiscalPeriodEnding}, Currency: {Currency}, Total Revenue: {TotalRevenue}, Net Income: {NetIncome}, Net Income Margin Percent: {NetIncomeMarginPercent}";
    }
}

public class NewsData
{
    public string Headline { get; set; }
    public string Source { get; set; }
}

public class SummaryData
{
    public string AsOfDate { get; set; }
    public string FiscalYear { get; set; }
    public double? revenue { get; set; }
    public double? new_assignments { get; set; }
    public double PNBs { get; set; }
    public double BDs { get; set; }
}

public class TopExecutive
{
    public string Name { get; set; }
    public string Role { get; set; }
    public int? Age { get; set; }
    public int? Tenure { get; set; }
}