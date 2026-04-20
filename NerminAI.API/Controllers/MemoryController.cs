using Microsoft.AspNetCore.Mvc;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;

namespace NerminAI.API.Controllers
{
    [ApiController]
    [Route("api/memory")]
    public class MemoryController : ControllerBase
    {
        private readonly IMemoryService _memoryService;

        public MemoryController(IMemoryService memoryService)
        {
            _memoryService = memoryService;
        }

        /// <summary>
        /// Add a new personal memory to the knowledge base.
        /// The memory is stored structurally and embedded for RAG retrieval.
        /// </summary>
        [HttpPost("add")]
        public async Task<ActionResult<AddMemoryResponse>> Add([FromBody] AddMemoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Title and Content are required.");

            var response = await _memoryService.AddMemoryAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Reflect on memories — filter by date range, category, emotional tone,
        /// or provide a free-form prompt for semantic search.
        /// </summary>
        [HttpPost("reflect")]
        public async Task<ActionResult<MemoryReflectionResponse>> Reflect([FromBody] MemoryReflectionRequest request)
        {
            var response = await _memoryService.ReflectAsync(request);
            return Ok(response);
        }
    }
}
