namespace Leaderboard_API.Models
{
    public class ScoreSubmissionResponse
    {
        public string Message { get; set; } = string.Empty;
        public int Rank { get; set; }
        public int TotalPlayers { get; set; }
    }
}