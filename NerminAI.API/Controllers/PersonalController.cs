using Microsoft.AspNetCore.Mvc;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;

namespace NerminAI.API.Controllers
{
    [ApiController]
    [Route("api/personal")]
    public class PersonalController : ControllerBase
    {
        private readonly IPersonalQuestionService _service;

        public PersonalController(IPersonalQuestionService service)
        {
            _service = service;
        }

        /// <summary>
        /// Ask the personal AI a question about the person's life, memories, preferences, or personality.
        /// </summary>
        [HttpPost("ask")]
        public async Task<ActionResult<PersonalQuestionResponse>> Ask([FromBody] PersonalQuestionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Question cannot be empty.");

            var response = await _service.AskAsync(request);
            return Ok(response);
        }
    }
}
