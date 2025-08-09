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
            _logger.LogInformation("Received submit-score request");

            try
            {
                // Log the incoming request details
                _logger.LogInformation("Request details - PlayerName: {PlayerName}, PlayerScore: {PlayerScore}",
                    request?.PlayerName ?? "NULL", request?.PlayerScore ?? 0);

                if (request == null)
                {
                    _logger.LogWarning("Request is null");
                    return BadRequest("Invalid request body");
                }

                if (string.IsNullOrWhiteSpace(request.PlayerName))
                {
                    _logger.LogWarning("PlayerName is null or empty: {PlayerName}", request.PlayerName);
                    return BadRequest("PlayerName is required");
                }

                _logger.LogInformation("Creating record for player: {PlayerName} with score: {PlayerScore}",
                    request.PlayerName, request.PlayerScore);

                var record = new Record
                {
                    PlayerName = request.PlayerName,
                    PlayerScore = request.PlayerScore
                };

                _logger.LogInformation("Record created successfully, calling MongoDB service");

                var success = await _mongoDbService.AddRecordAsync(record);

                _logger.LogInformation("MongoDB service returned: {Success}", success);

                if (success)
                {
                    _logger.LogInformation("Score submitted successfully for player: {PlayerName}", request.PlayerName);
                    return Ok("Score submitted successfully");
                }
                else
                {
                    _logger.LogError("MongoDB service returned false for player: {PlayerName}", request.PlayerName);
                    return StatusCode(500, "Failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while submitting score for player: {PlayerName}. Exception type: {ExceptionType}, Message: {Message}",
                    request?.PlayerName ?? "UNKNOWN", ex.GetType().Name, ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerExceptionType}, Message: {InnerMessage}",
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                }

                return StatusCode(500, "Failed");
            }
        }
    }
}