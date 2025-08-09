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
    }
}