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

        public ReportGenerationController(Kernel kernel)
        {
            _kernel = kernel;
        }

        [HttpPost()]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] string chatRequest)
        {
            return Ok();
        }
    }
}