using System.ComponentModel.DataAnnotations;

public class AzureOpenAiOptions
{
    public const string AzureOpenAI = "AzureOpenAiOptions";

    [Required]
    public string DeploymentName { get; set; }
    [Required]
    public string EndPoint { get; set; }
    [Required]
    public string ApiKey { get; set; }
}