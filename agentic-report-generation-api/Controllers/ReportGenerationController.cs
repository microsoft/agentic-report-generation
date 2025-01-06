using AgenticReportGenerationApi.Models;
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

        public ReportGenerationController(
            Kernel kernel,
            IChatCompletionService chat,
            ILogger<ReportGenerationController> logger)
        {
            _kernel = kernel;
            _logger = logger;
            _chat = chat;
        }

        [HttpPost()]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] ReportGenerationRequest chatRequest)
        {
            var schema = CompanySchema();
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