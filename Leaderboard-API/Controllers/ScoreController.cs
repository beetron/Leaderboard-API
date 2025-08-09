using Leaderboard_API.Models;
using Leaderboard_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScoreController : ControllerBase
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly ILogger<ScoreController> _logger;

        public ScoreController(IMongoDbService mongoDbService, ILogger<ScoreController> logger)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
        }

        [HttpPost("submit-score")]
        public async Task<IActionResult> SubmitScore([FromBody] ScoreSubmissionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PlayerName))
                {
                    return BadRequest("PlayerName is required");
                }

                var record = new Record
                {
                    PlayerName = request.PlayerName,
                    PlayerScore = request.PlayerScore
                };

                var success = await _mongoDbService.AddRecordAsync(record);

                if (success)
                {
                    _logger.LogInformation("Score submitted successfully for player: {PlayerName}", request.PlayerName);
                    return Ok("Score submitted successfully");
                }
                else
                {
                    _logger.LogError("Failed to submit score for player: {PlayerName}", request.PlayerName);
                    return StatusCode(500, "Failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while submitting score for player: {PlayerName}", request.PlayerName);
                return StatusCode(500, "Failed");
            }
        }
    }
}