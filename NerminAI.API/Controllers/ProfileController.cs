using Microsoft.AspNetCore.Mvc;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;

namespace NerminAI.API.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        /// <summary>
        /// Get the active personal profile.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ProfileDto>> GetProfile()
        {
            var profile = await _profileService.GetActiveProfileAsync();
            if (profile is null)
                return NotFound("No active profile found. Use PUT /api/profile to create one.");
            return Ok(profile);
        }

        /// <summary>
        /// Create or update the personal profile.
        /// Previous profile is deactivated automatically.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpsertProfile([FromBody] UpsertProfileRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Bio))
                return BadRequest("FullName and Bio are required.");

            await _profileService.UpsertProfileAsync(request);
            return Ok(new { message = "Profile updated successfully." });
        }

        /// <summary>
        /// Get a personality summary including traits, interests breakdown, and an LLM narrative.
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<PersonalitySummaryResponse>> GetSummary()
        {
            var summary = await _profileService.GetPersonalitySummaryAsync();
            return Ok(summary);
        }
    }
}
