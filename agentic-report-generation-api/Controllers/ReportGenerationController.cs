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
        private readonly ChatHistoryManager _chatHistoryManager;
        private readonly CompanyNameChatHistoryManager _companyNameChatHistoryManager;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IMemoryCache _memoryCache;

        public ReportGenerationController(
            Kernel kernel,
            IChatCompletionService chat,
            ChatHistoryManager chatHistorymanager,
            CompanyNameChatHistoryManager companyNameChatHistoryManager,
            ILogger<ReportGenerationController> logger,
            ICosmosDbService cosmosDbService,
            IMemoryCache memoryCache)
        {
            _kernel = kernel;
            _logger = logger;
            _chat = chat;
            _chatHistoryManager = chatHistorymanager;
            _companyNameChatHistoryManager = companyNameChatHistoryManager;
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
                var companyNameChatHistory = _companyNameChatHistoryManager.GetOrCreateChatHistory(sessionId);
                var companyNamesPrompt = string.Empty;

                chatHistory.AddUserMessage(chatRequest.Prompt);

                // check if we have the company id in the chat history so we know whether or not we should
                // try and extract the company name
                bool hasCompanyId = await Util.HasCompanyId(_chat, chatHistory);

                if (!string.IsNullOrEmpty(chatRequest.CompanyId) || hasCompanyId)
                {
                    var jsonObject = new JObject
                    {
                        { "company_id", chatRequest.CompanyId },
                        { "company_name", chatRequest.CompanyName }
                    };

                    await CacheCompanyAsync(jsonObject.ToString());

                    // if we found the company id is already in the chat history, we retrieved it from the get company name utility and don't
                    // need to add it again
                    if (!hasCompanyId)
                    {
                        chatHistory.AddUserMessage(jsonObject.ToString());
                    }
                }
                else
                {
                    // only add the system message the first time so we do not have multiple system messages added to the chat history
                    if (companyNameChatHistory.Count == 0)
                    {
                        var jsonCompanyNames = await GetCompanyIdAndNameAsync();
                        companyNamesPrompt = CorePrompts.GetCompanyPrompt(jsonCompanyNames);
                        companyNameChatHistory.AddSystemMessage(companyNamesPrompt);
                    }

                    // add user prompt to chat history
                    companyNameChatHistory.AddUserMessage(chatRequest.Prompt);

                    // Get company name from prompt
                    var jsonCompanyResponse = await Util.GetCompanyName(_chat, companyNameChatHistory);

                    if (jsonCompanyResponse.Contains("not_found"))
                    {
                        _logger.LogWarning("Company name not found in prompt.");

                        // remove "not_found" from the response
                        jsonCompanyResponse = jsonCompanyResponse.Remove(jsonCompanyResponse.IndexOf("not_found"), "not_found".Length);

                        companyNameChatHistory.AddSystemMessage(jsonCompanyResponse);
                        chatHistory.AddSystemMessage(jsonCompanyResponse);

                        return new OkObjectResult(jsonCompanyResponse);
                    }
                    else if (jsonCompanyResponse.Contains("choose_company"))
                    {
                        _logger.LogInformation("Multiple similar company names detected.");

                        // remove "choose_company" from the response
                        jsonCompanyResponse = jsonCompanyResponse.Remove(jsonCompanyResponse.IndexOf("choose_company"), "choose_company".Length);

                        companyNameChatHistory.AddSystemMessage(jsonCompanyResponse);
                        chatHistory.AddSystemMessage(jsonCompanyResponse);

                        return new OkObjectResult(jsonCompanyResponse);
                    }

                    // add the JSON of the selected company to the chat history so if a follow up question is asked, we can use it
                    chatHistory.AddUserMessage(jsonCompanyResponse);

                    await CacheCompanyAsync(jsonCompanyResponse);
                }

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