namespace Leaderboard_API.Models
{
    public class GetRankingsResponse
    {
        public List<LeaderboardEntry> Rankings { get; set; } = new List<LeaderboardEntry>();
        public int TotalCount { get; set; }
        public int RequestedCount { get; set; }
    }
}