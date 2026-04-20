using Microsoft.AspNetCore.Mvc;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;

namespace NerminAI.API.Controllers
{
    [ApiController]
    [Route("api/recommend")]
    public class RecommendController : ControllerBase
    {
        private readonly IRecommendService _recommendService;

        public RecommendController(IRecommendService recommendService)
        {
            _recommendService = recommendService;
        }

        /// <summary>
        /// Get personalized activity recommendations based on interests and preferences.
        /// Optionally provide a context string to tailor recommendations.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RecommendationResponse>> Recommend([FromBody] RecommendationRequest request)
        {
            var response = await _recommendService.RecommendAsync(request);
            return Ok(response);
        }
    }
}
