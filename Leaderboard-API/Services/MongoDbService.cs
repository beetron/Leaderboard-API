using Leaderboard_API.Models;
using MongoDB.Driver;

namespace Leaderboard_API.Services
{
    public class MongoDbService : IMongoDbService
    {
        private readonly IMongoCollection<Record> _recordsCollection;
        private readonly ILogger<MongoDbService> _logger;

        public MongoDbService(IConfiguration configuration, ILogger<MongoDbService> logger)
        {
            _logger = logger;

            // Read connection string from Docker secret file
            var connectionString = ReadDockerSecret("mongo_connection_string");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("MongoDB connection string not found in Docker secrets");
                throw new InvalidOperationException("MongoDB connection string not found in Docker secrets");
            }

            // Database and collection names are safe to expose as environment variables
            var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME") ?? "leaderboard";
            var collectionName = Environment.GetEnvironmentVariable("MONGODB_COLLECTION_NAME") ?? "records";

            _logger.LogInformation("Connecting to MongoDB database: {DatabaseName}, collection: {CollectionName}",
                databaseName, collectionName);

            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);
                _recordsCollection = database.GetCollection<Record>(collectionName);

                _logger.LogInformation("MongoDB connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish MongoDB connection: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private string ReadDockerSecret(string secretName)
        {
            try
            {
                var secretPath = $"/run/secrets/{secretName}";

                if (File.Exists(secretPath))
                {
                    var secretValue = File.ReadAllText(secretPath).Trim();
                    _logger.LogInformation("Successfully read Docker secret: {SecretName}", secretName);
                    return secretValue;
                }
                else
                {
                    _logger.LogWarning("Docker secret file not found: {SecretPath}", secretPath);

                    // Fallback to environment variable for development/testing
                    var envValue = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
                    if (!string.IsNullOrEmpty(envValue))
                    {
                        _logger.LogInformation("Using environment variable as fallback for connection string");
                        return envValue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Docker secret: {SecretName}", secretName);
            }

            return string.Empty;
        }

        public async Task<bool> AddRecordAsync(Record record)
        {
            try
            {
                _logger.LogInformation("Attempting to insert record for player: {PlayerName}", record.PlayerName);
                await _recordsCollection.InsertOneAsync(record);
                _logger.LogInformation("Successfully inserted record for player: {PlayerName}", record.PlayerName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert record for player: {PlayerName}. Error: {ErrorMessage}",
                    record.PlayerName, ex.Message);
                return false;
            }
        }

        public async Task<int> GetPlayerRankAsync(int playerScore, DateTime createdAt)
        {
            try
            {
                _logger.LogInformation("Calculating rank for score: {PlayerScore} created at: {CreatedAt}",
                    playerScore, createdAt);

                // Count players with higher scores
                var higherScoreFilter = Builders<Record>.Filter.Gt(r => r.PlayerScore, playerScore);
                var playersWithHigherScores = await _recordsCollection.CountDocumentsAsync(higherScoreFilter);

                // Count players with same score but earlier CreatedAt (they rank higher)
                var sameScoreEarlierFilter = Builders<Record>.Filter.And(
                    Builders<Record>.Filter.Eq(r => r.PlayerScore, playerScore),
                    Builders<Record>.Filter.Lt(r => r.CreatedAt, createdAt)
                );
                var playersWithSameScoreButEarlier = await _recordsCollection.CountDocumentsAsync(sameScoreEarlierFilter);

                // Rank = players with higher scores + players with same score but earlier time + 1
                var rank = (int)(playersWithHigherScores + playersWithSameScoreButEarlier) + 1;

                _logger.LogInformation("Player with score {PlayerScore} (created {CreatedAt}) is ranked {Rank}. " +
                    "Higher scores: {HigherScores}, Same score but earlier: {SameScoreEarlier}",
                    playerScore, createdAt, rank, playersWithHigherScores, playersWithSameScoreButEarlier);

                return rank;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate rank for score: {PlayerScore}. Error: {ErrorMessage}",
                    playerScore, ex.Message);
                return -1; // Return -1 to indicate error
            }
        }

        public async Task<int> GetTotalPlayersCountAsync()
        {
            try
            {
                var totalPlayers = await _recordsCollection.CountDocumentsAsync(FilterDefinition<Record>.Empty);
                _logger.LogInformation("Total players in leaderboard: {TotalPlayers}", totalPlayers);
                return (int)totalPlayers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get total players count. Error: {ErrorMessage}", ex.Message);
                return -1; // Return -1 to indicate error
            }
        }

        public async Task<List<Record>> GetTopScoresAsync(int count)
        {
            try
            {
                _logger.LogInformation("Retrieving top {Count} scores", count);

                // Sort by PlayerScore descending (highest first), then by CreatedAt ascending (earliest first for ties)
                var sort = Builders<Record>.Sort.Descending(r => r.PlayerScore).Ascending(r => r.CreatedAt);

                // Get the top scores
                var topScores = await _recordsCollection
                    .Find(FilterDefinition<Record>.Empty)
                    .Sort(sort)
                    .Limit(count)
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {ActualCount} out of requested {RequestedCount} top scores",
                    topScores.Count, count);

                return topScores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get top {Count} scores. Error: {ErrorMessage}", count, ex.Message);
                return new List<Record>();
            }
        }
    }
}