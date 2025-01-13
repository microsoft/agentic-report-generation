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
    [JsonProperty(PropertyName = "rra_activity")]
    public List<RraActivity> rra_activity { get; set; }
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
    public double? NetIncome_Margin_Percent { get; set; }
    public string GrowthOverPriorYear { get; set; }
    public string EBTExclUnusualItems { get; set; }
    public string MarginPercent_EBT { get; set; }
    public string EarningsFromContOps { get; set; }
    public string MarginPercent_Earnings { get; set; }
    public double? MarginPercent_NetIncome { get; set; }
    public string DilutedEPS { get; set; }
    public string GrowthOverPriorYear_EPS { get; set; }
    public string TotalAssets { get; set; }
    public string GrowthOverPriorYear_Assets { get; set; }

    public override string ToString()
    {
        return $"FiscalPeriodEnding: {FiscalPeriodEnding}, Currency: {Currency}, TotalRevenue: {TotalRevenue}, NetIncome: {NetIncome}, NetIncome_Margin_Percent: {NetIncome_Margin_Percent}, GrowthOverPriorYear: {GrowthOverPriorYear}, EBTExclUnusualItems: {EBTExclUnusualItems}, MarginPercent_EBT: {MarginPercent_EBT}, EarningsFromContOps: {EarningsFromContOps}, MarginPercent_Earnings: {MarginPercent_Earnings}, MarginPercent_NetIncome: {MarginPercent_NetIncome}, DilutedEPS: {DilutedEPS}, GrowthOverPriorYear_EPS: {GrowthOverPriorYear_EPS}, TotalAssets: {TotalAssets}, GrowthOverPriorYear_Assets: {GrowthOverPriorYear_Assets}";
    }
}

public class NewsData
{
    public string Headline { get; set; }
    public string Source { get; set; }
}

public class RraActivity
{
    public string category { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> DynamicFields { get; set; } = new Dictionary<string, object>();
}

public class TopExecutive
{
    public string Name { get; set; }
    public string Role { get; set; }
    public int? Age { get; set; }
    public int? Tenure { get; set; }
}