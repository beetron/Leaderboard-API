using Leaderboard_API.Models;
using MongoDB.Driver;

namespace Leaderboard_API.Services
{
    public class MongoDbService : IMongoDbService
    {
        private readonly IMongoCollection<Record> _recordsCollection;

        public MongoDbService(IConfiguration configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
                ?? throw new InvalidOperationException("MongoDB connection string not found in environment variables");

            var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME") ?? "leaderboard";
            var collectionName = Environment.GetEnvironmentVariable("MONGODB_COLLECTION_NAME") ?? "records";

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _recordsCollection = database.GetCollection<Record>(collectionName);
        }

        public async Task<bool> AddRecordAsync(Record record)
        {
            try
            {
                await _recordsCollection.InsertOneAsync(record);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}