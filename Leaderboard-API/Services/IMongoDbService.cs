using Leaderboard_API.Models;

namespace Leaderboard_API.Services
{
    public interface IMongoDbService
    {
        Task<bool> AddRecordAsync(Record record);
        Task<int> GetPlayerRankAsync(int playerScore);
        Task<int> GetTotalPlayersCountAsync();
        Task<List<Record>> GetTopScoresAsync(int count);
    }
}