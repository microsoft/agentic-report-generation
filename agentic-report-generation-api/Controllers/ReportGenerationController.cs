﻿using AgenticReportGenerationApi.Models;
using AgenticReportGenerationApi.Services;
using EntertainmentChatApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json.Schema.Generation;
using System.Net.Mime;

namespace AgenticReportGenerationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

                // Get company name from prompt
                var companyName = await Util.GetCompanyName(_chat, chatRequest.Prompt);

                if (companyName == "not_found")
                {
                    _logger.LogWarning("Company name not found in prompt.");
                    return new BadRequestResult();
                }

                // TODO: Injected into additional system prompt
                var companyNames = await GetCompanyNamesAsync();

                // Cache the company data
                await CacheCompanyAsync(companyName);
                
                chatHistory.AddUserMessage(chatRequest.Prompt);

                ChatMessageContent? result = null;
                result = await _chat.GetChatMessageContentAsync(
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

        [HttpPost("create-company")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCompany([FromBody] Company company)
        {
            try
            {
               if (company == null)
               {
                    return new BadRequestResult();
               }

               company.Id = Guid.NewGuid().ToString();
               await _cosmosDbService.AddAsync(company);
               return Ok();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error processing request.");
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> Get(string id, [FromQuery] string companyName)
        {
            var company = await _cosmosDbService.GetAsync(id, companyName);
            return Ok(company);
        }

        [HttpGet("by-name/{companyName}")]
        public async Task<IActionResult> Get(string companyName)
        {
            var company = await _cosmosDbService.GetAsync(companyName);
            return Ok(company);
        }

        // TODO: May be needed for NL2SQL
        private static string CompanySchema()
        {
            var generator = new JSchemaGenerator();
            var schema = generator.Generate(typeof(Company));
            return schema.ToString();
        }

        private async Task CacheCompanyAsync(string companyName)
        {
            if (!_memoryCache.TryGetValue(companyName, out Company? company))
            {
                company = await _cosmosDbService.GetAsync(companyName);

                if (company == null)
                {
                    _logger.LogWarning($"Company '{companyName}' not found in database.");
                    throw new InvalidOperationException($"Company '{companyName}' not found in database.");
                }
                else
                {
                    _memoryCache.Set(companyName, company, TimeSpan.FromMinutes(120));
                }
            }
        }

        private async Task<List<string>> GetCompanyNamesAsync()
        {
            if (!_memoryCache.TryGetValue("companyNames", out List<string> companyNames))
            {
                companyNames = await _cosmosDbService.GetCompanyNamesAsync();
                _memoryCache.Set("companyNames", companyNames, TimeSpan.FromMinutes(120));
            }

            return companyNames;
        }
    }
}