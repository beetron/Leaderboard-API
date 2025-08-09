namespace Leaderboard_API.Models
{
    public class ScoreSubmissionRequest
    {
        public string PlayerName { get; set; } = string.Empty;
        public int PlayerScore { get; set; }
    }
}