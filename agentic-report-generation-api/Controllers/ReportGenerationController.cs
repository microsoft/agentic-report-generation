using AgenticReportGenerationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using System.Net.Mime;

namespace AgenticReportGenerationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportGenerationController : ControllerBase
    {
        private readonly Kernel _kernel;
        private readonly ILogger<ReportGenerationController> _logger;

        public ReportGenerationController(
            Kernel kernel, 
            ILogger<ReportGenerationController> logger)
        {
            _kernel = kernel;
            _logger = logger;
        }

        [HttpPost()]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] ReportGenerationRequest chatRequest)
        {
            return Ok();
        }
    }
}