using Leaderboard_API.Models;

namespace Leaderboard_API.Services
{
    public interface IMongoDbService
    {
        Task<bool> AddRecordAsync(Record record);
    }
}