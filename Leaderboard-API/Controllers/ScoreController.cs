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

                    // Calculate the player's rank
                    var rank = await _mongoDbService.GetPlayerRankAsync(request.PlayerScore);
                    var totalPlayers = await _mongoDbService.GetTotalPlayersCountAsync();

                    if (rank > 0 && totalPlayers > 0)
                    {
                        _logger.LogInformation("Player {PlayerName} ranked {Rank} out of {TotalPlayers}",
                            request.PlayerName, rank, totalPlayers);

                        var response = new ScoreSubmissionResponse
                        {
                            Message = "Score submitted successfully",
                            Rank = rank,
                            TotalPlayers = totalPlayers
                        };

                        return Ok(response);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to calculate rank or total players for {PlayerName}", request.PlayerName);
                        return Ok("Score submitted successfully, but ranking could not be calculated");
                    }
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

        [HttpGet("get-rankings")]
        public async Task<IActionResult> GetRankings([FromQuery] int count = 10)
        {
            _logger.LogInformation("Received get-rankings request for top {Count} scores", count);

            try
            {
                // Validate the count parameter
                if (count <= 0)
                {
                    _logger.LogWarning("Invalid count parameter: {Count}", count);
                    return BadRequest("Count must be greater than 0");
                }

                // Set a reasonable maximum limit to prevent performance issues
                if (count > 1000)
                {
                    _logger.LogWarning("Count parameter too large: {Count}, limiting to 1000", count);
                    count = 1000;
                }

                _logger.LogInformation("Fetching top {Count} scores from MongoDB", count);

                // Get the top scores from MongoDB
                var topScores = await _mongoDbService.GetTopScoresAsync(count);
                var totalPlayers = await _mongoDbService.GetTotalPlayersCountAsync();

                if (topScores == null)
                {
                    _logger.LogError("Failed to retrieve top scores from MongoDB");
                    return StatusCode(500, "Failed to retrieve rankings");
                }

                // Convert to leaderboard entries with ranks
                var rankings = topScores.Select((record, index) => new LeaderboardEntry
                {
                    Rank = index + 1,
                    PlayerName = record.PlayerName,
                    PlayerScore = record.PlayerScore,
                    CreatedAt = record.CreatedAt
                }).ToList();

                var response = new GetRankingsResponse
                {
                    Rankings = rankings,
                    TotalCount = totalPlayers > 0 ? totalPlayers : rankings.Count,
                    RequestedCount = count
                };

                _logger.LogInformation("Successfully retrieved {ActualCount} rankings out of {RequestedCount} requested",
                    rankings.Count, count);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting rankings. Exception type: {ExceptionType}, Message: {Message}",
                    ex.GetType().Name, ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerExceptionType}, Message: {InnerMessage}",
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                }

                return StatusCode(500, "Failed to retrieve rankings");
            }
        }
    }
}