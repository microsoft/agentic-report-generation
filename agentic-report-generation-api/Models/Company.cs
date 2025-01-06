using Newtonsoft.Json;

public class Company
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }
    [JsonProperty(PropertyName = "CompanyName")]
    public string CompanyName { get; set; }
    public string CompanyDescription { get; set; }
    public int? Revenue { get; set; }
    public int? MarketCap { get; set; }
    public List<BoardMember> BoardMembers { get; set; }
    public List<TopExecutive> TopExecutives { get; set; }
    public List<CorporateTimeline> CorporateTimelines { get; set; }
    public List<FinancialDatum> FinancialData { get; set; }
    public List<NewsData> NewsData { get; set; }
    public List<SummaryData> SummaryData { get; set; }
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

public class FinancialDatum
{
    public string FiscalPeriodEnding { get; set; }
    public string Currency { get; set; }
    public double? TotalRevenue { get; set; }
    public double? NetIncome { get; set; }
    public double? NetIncomeMarginPercent { get; set; }
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
}

public class TopExecutive
{
    public string Name { get; set; }
    public string Role { get; set; }
    public int? Age { get; set; }
    public int? Tenure { get; set; }
}