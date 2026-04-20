using Microsoft.AspNetCore.Mvc;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;

namespace NerminAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AskController : ControllerBase
    {
        private readonly IAskService _askService;

        public AskController(IAskService askService)
        {
            _askService = askService;
        }

        [HttpPost]
        public async Task<ActionResult<AskResponse>> Ask([FromBody] AskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Question cannot be empty.");

            var response = await _askService.AskAsync(request);
            return Ok(response);
        }
    }
}
