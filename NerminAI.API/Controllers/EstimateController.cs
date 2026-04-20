using Microsoft.AspNetCore.Mvc;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;

namespace NerminAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstimateController : ControllerBase
    {
        private readonly IEstimateService _estimateService;

        public EstimateController(IEstimateService estimateService)
        {
            _estimateService = estimateService;
        }

        [HttpPost]
        public async Task<ActionResult<EstimateResponse>> Estimate([FromBody] EstimateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ProjectDescription))
                return BadRequest("Project description cannot be empty.");

            var response = await _estimateService.EstimateAsync(request);
            return Ok(response);
        }
    }
}
