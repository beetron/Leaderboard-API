namespace Leaderboard_API.Models
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int PlayerScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}