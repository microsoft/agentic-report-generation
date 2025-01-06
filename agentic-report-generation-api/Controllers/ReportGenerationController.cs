using AgenticReportGenerationApi.Models;
using EntertainmentChatApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Data;
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

        public ReportGenerationController(
            Kernel kernel,
            IChatCompletionService chat,
            IChatHistoryManager chathistorymanager,
            ILogger<ReportGenerationController> logger)
        {
            _kernel = kernel;
            _logger = logger;
            _chat = chat;
            _chatHistoryManager = chathistorymanager;
        }

        [HttpPost()]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] ReportGenerationRequest chatRequest)
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

                var schema = CompanySchema();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
                        
            return Ok();
        }

        private static string CompanySchema()
        {
            var generator = new JSchemaGenerator();
            var schema = generator.Generate(typeof(Company));
            return schema.ToString();
        }
    }
}