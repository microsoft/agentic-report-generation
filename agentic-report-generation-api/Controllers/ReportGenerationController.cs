using AgenticReportGenerationApi.Models;
using AgenticReportGenerationApi.Prompts;
using AgenticReportGenerationApi.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json.Schema.Generation;
using System.Net.Mime;
using System.Text.Json;
using AgenticReportGenerationApi.Converters;
using Newtonsoft.Json.Linq;

namespace AgenticReportGenerationApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{v:apiVersion}/[controller]")]
    public class ReportGenerationController : ControllerBase
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chat;
        private readonly ILogger<ReportGenerationController> _logger;
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IMemoryCache _memoryCache;

        public ReportGenerationController(
            Kernel kernel,
            IChatCompletionService chat,
            IChatHistoryManager chathistorymanager,
            ILogger<ReportGenerationController> logger,
            ICosmosDbService cosmosDbService,
            IMemoryCache memoryCache)
        {
            _kernel = kernel;
            _logger = logger;
            _chat = chat;
            _chatHistoryManager = chathistorymanager;
            _cosmosDbService = cosmosDbService;
            _memoryCache = memoryCache;
        }

        [MapToApiVersion("1.0")]
        [HttpPost("report-generator")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateReport([FromBody] ReportGenerationRequest chatRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(chatRequest.SessionId))
                {
                    // needed for new chats
                    chatRequest.SessionId = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(chatRequest.Prompt))
                {
                    _logger.LogWarning("Chat request is missing prompt.");
                    return new BadRequestResult();
                }

                var sessionId = chatRequest.SessionId;
                var chatHistory = _chatHistoryManager.GetOrCreateChatHistory(sessionId);
                var companyNamesPrompt = string.Empty;

                if (!string.IsNullOrEmpty(chatRequest.CompanyId))
                {
                    var jsonObject = new JObject
                    {
                        { "company_id", chatRequest.CompanyId },
                        { "company_name", chatRequest.CompanyName }
                    };

                    await CacheCompanyAsync(jsonObject.ToString());
                    chatHistory.AddUserMessage(jsonObject.ToString());
                }
                else
                {
                    var jsonCompanyNames = await GetCompanyIdAndNameAsync();
                    companyNamesPrompt = CorePrompts.GetCompanyPrompt(jsonCompanyNames);

                    // Get company name from prompt
                    var jsonCompanyResponse = await Util.GetCompanyName(_chat, chatRequest.Prompt, companyNamesPrompt);

                    if (jsonCompanyResponse.Contains("not_found"))
                    {
                        _logger.LogWarning("Company name not found in prompt.");
                        return new BadRequestResult();
                    }
                    else if (jsonCompanyResponse.Contains("choose_company"))
                    {
                        _logger.LogInformation("Multiple similar company names detected.");
                        return new OkObjectResult(jsonCompanyResponse);
                    }

                    await CacheCompanyAsync(jsonCompanyResponse);
                    chatHistory.AddSystemMessage(companyNamesPrompt);
                }

                chatHistory.AddUserMessage(chatRequest.Prompt);

                ChatMessageContent? result = await _chat.GetChatMessageContentAsync(
                      chatHistory,
                      executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.0, TopP = 0.0, ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions },
                      kernel: _kernel);

                return new OkObjectResult(result.Content);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, $"Error processing request. {ex.Message}");
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [MapToApiVersion("1.0")]
        [HttpPost("create-company")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCompany([FromBody] JsonElement companyJson)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new RraActivityJsonConverter() }
                };

                var company = JsonSerializer.Deserialize<Company>(companyJson.GetRawText(), options);

                if (company == null)
                {
                    return new BadRequestResult();
                }

                company.Id = Guid.NewGuid().ToString();
                await _cosmosDbService.AddAsync(company);
                return Ok();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing JSON.");
                return BadRequest("Invalid JSON format.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error processing request.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [MapToApiVersion("1.0")]
        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> Get(string id, [FromQuery] string companyName)
        {
            var company = await _cosmosDbService.GetAsync(id, companyName);
            return Ok(company);
        }

        [MapToApiVersion("1.0")]
        [HttpGet("by-name/{companyName}")]
        public async Task<IActionResult> Get(string companyName)
        {
            var company = await _cosmosDbService.GetCompanyByNameAsync(companyName);
            return Ok(company);
        }

        [MapToApiVersion("1.0")]
        [HttpGet("all-companies")]
        public async Task<IActionResult> GetAllCompanies()
        {
            try
            {
                var companies = await _cosmosDbService.GetAllCompaniesAsync();
                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving companies.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // TODO: May be needed for NL2SQL
        private static string CompanySchema()
        {
            var generator = new JSchemaGenerator();
            var schema = generator.Generate(typeof(Company));
            return schema.ToString();
        }

        /// <summary>
        /// Looks up a company name in the cache or pulls it from the database and caches it.
        /// </summary>
        /// <param name="jsonCompany">JSON structure of a company in the format:
        /// {
        ///    "company_name": Microsoft,
        ///    "company_id": 123456
        /// }
        /// </param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task CacheCompanyAsync(string jsonCompany)
        {
            var jsonObject = JObject.Parse(jsonCompany);
            var companyId = jsonObject["company_id"].ToString();
            var companyName = jsonObject["company_name"].ToString();

            if (!_memoryCache.TryGetValue(companyId, out Company? company))
            {
                company = await _cosmosDbService.GetCompanyByIdAsync(companyId);

                if (company == null)
                {
                    _logger.LogWarning($"Company '{companyName}', with company ID '{companyId}' not found in database.");
                    throw new InvalidOperationException($"Company '{companyName}', with company ID '{companyId}' not found in database.");
                }
                else
                {
                    _memoryCache.Set(companyId, company, TimeSpan.FromMinutes(120));
                }
            }
        }

        private async Task<string> GetCompanyNamesAsync()
        {
            if (!_memoryCache.TryGetValue("companyNames", out List<string> companyNames))
            {
                companyNames = await _cosmosDbService.GetCompanyNamesAsync();
                _memoryCache.Set("companyNames", companyNames, TimeSpan.FromMinutes(120));
            }

            var serialized = string.Join("| ", companyNames);
            return serialized;
        }

        private async Task<string> GetCompanyIdAndNameAsync()
        {
            if (!_memoryCache.TryGetValue("CompanyIdAndName", out Dictionary<string, string> companyIdAndName))
            {
                companyIdAndName = await _cosmosDbService.GetCompanyIdAndNameAsync();
                _memoryCache.Set("CompanyIdAndName", companyIdAndName, TimeSpan.FromMinutes(120));
            }

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var jsonString = JsonSerializer.Serialize(companyIdAndName, jsonOptions);
            return jsonString;
        }

        [NonAction]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });
        }

        [NonAction]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("AllowAllOrigins");
        }
    }
}