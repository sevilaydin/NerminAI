using Microsoft.AspNetCore.Mvc;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;

namespace NerminAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ISeedService _seedService;
        private readonly ISeedPersonalDataService _seedPersonalDataService;

        public SeedController(ISeedService seedService, ISeedPersonalDataService seedPersonalDataService)
        {
            _seedService = seedService;
            _seedPersonalDataService = seedPersonalDataService;
        }

        [HttpPost]
        public async Task<IActionResult> Seed([FromBody] SeedRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Title and Content are required.");

            await _seedService.SeedDocumentAsync(request);
            return Ok(new { message = "Document seeded successfully." });
        }

        [HttpPost("default")]
        public async Task<IActionResult> SeedDefault()
        {
            await _seedService.SeedDefaultDataAsync();
            return Ok(new { message = "Default knowledge base seeded successfully." });
        }

        [HttpPost("chat-export")]
        public async Task<IActionResult> SeedChatExport([FromBody] SeedChatExportRequest? request = null)
        {
            var importedCount = await _seedService.SeedChatExportAsync(
                request?.ExportDirectory,
                request?.KeywordRegex,
                request?.IncludeAll ?? false);

            return Ok(new
            {
                message = "Chat export knowledge seeded successfully.",
                importedConversations = importedCount
            });
        }

        /// <summary>
        /// Seed personal data (profile, memories, preferences, relationships, events, interests, routines).
        /// Each entity is stored structurally AND embedded as a shadow document for RAG retrieval.
        /// </summary>
        [HttpPost("personal")]
        public async Task<IActionResult> SeedPersonal([FromBody] SeedPersonalDataRequest request)
        {
            await _seedPersonalDataService.SeedAsync(request);
            return Ok(new { message = "Personal data seeded successfully." });
        }
    }
}
