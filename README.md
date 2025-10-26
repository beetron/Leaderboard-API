# Leaderboard API

A robust RESTful API for managing game leaderboards, built with .NET 9 and MongoDB. This service handles score submissions, rankings retrieval, and leaderboard management with health monitoring capabilities.

**Created for**: [Mini Miner](https://lazy-onigiri.itch.io/mini-miner) - A browser-based mining game on itch.io

## Project Overview

**Leaderboard-API** is a containerized web service designed to manage game leaderboards efficiently. It provides endpoints for:
- **Score Submission**: Submit player scores with validation
- **Rankings Retrieval**: Fetch ranked leaderboard data
- **Health Checks**: Monitor API availability and status

### Key Features

- ✅ **RESTful API** built with ASP.NET Core 9.0
- 🗄️ **MongoDB Integration** for persistent data storage
- 🐳 **Docker Support** with multi-stage builds for optimized images
- 🔒 **CORS Configuration** for safe cross-origin requests (itch.zone integration)
- 🏥 **Health Checks** with automatic container restart policies
- ⚙️ **Environment-based Configuration** for flexible deployments
- 📊 **Production-Ready** with proper error handling and logging

## Prerequisites

Before you begin, ensure you have the following installed:

- **Git**: For cloning the repository
- **Docker & Docker Compose**: For containerized deployment
- **.NET 9 SDK** (optional): For local development without Docker
- **MongoDB**: Either local instance or remote connection string

## Quick Start Guide

### 1. Clone the Repository

```bash
git clone https://github.com/beetron/Leaderboard-API.git
cd Leaderboard-API
```

### 2. Setup Environment Variables

The project uses environment variables for configuration. Create or update the `.env` file in the `Leaderboard-API` directory:

```bash
# Leaderboard-API/.env
MONGO_CONNECTION_STRING=mongodb://username:password@host:port/
```

Replace the MongoDB connection string with your actual credentials and host information.

**Example for local MongoDB:**
```
MONGO_CONNECTION_STRING=mongodb://localhost:27017/
```

**Example for MongoDB Atlas (Cloud):**
```
MONGO_CONNECTION_STRING=mongodb+srv://username:password@cluster.mongodb.net/
```

### 3. Build the Docker Image

```bash
# Build the image
docker build -f Leaderboard-API/Dockerfile -t leaderboard-api:latest .
```

Or use Docker Compose to build automatically:

```bash
docker-compose build
```

### 4. Run the Application

#### Option A: Using Docker Compose (Recommended)

```bash
docker-compose up -d
```

The API will be available at `http://localhost:8090`

#### Option B: Using Docker Directly

```bash
docker run -d \
  --name leaderboard-api \
  -p 8090:8090 \
  --env-file ./Leaderboard-API/.env \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:8090 \
-e MONGODB_DATABASE_NAME=leaderboard \
  -e MONGODB_COLLECTION_NAME=records \
  leaderboard-api:latest
```

#### Option C: Local Development (Without Docker)

```bash
cd Leaderboard-API
dotnet restore
dotnet build
dotnet run
```

The API will be available at `http://localhost:5000` (or as configured in `launchSettings.json`)

## Configuration

### Port Configuration

The API runs on **port 8090** by default. To change the port, modify the following files:

#### For Docker Deployments:

**File**: `docker-compose.yml`
```yaml
services:
  leaderboard-api:
    ports:
      - target: 8090        # Container port
        published: 3000     # Change this to your desired port
        protocol: tcp
        mode: host
    environment:
      - ASPNETCORE_URLS=http://+:8090  # Keep this as the container port
```

**File**: `Leaderboard-API/Dockerfile`
```dockerfile
EXPOSE 8090  # Change to your desired port
```

#### For Local Development:

**File**: `Leaderboard-API/Properties/launchSettings.json`
```json
{
  "profiles": {
  "http": {
      "applicationUrl": "http://localhost:5000"  // Change 5000 to your port
    }
  }
}
```

#### For .NET Configuration:

**File**: `Leaderboard-API/appsettings.json`
```json
{
  "MongoDb": {
  "DatabaseName": "leaderboard",
    "CollectionName": "records"
  }
}
```

### Environment Variables Reference

| Variable | Description | Example |
|----------|-------------|---------|
| `MONGO_CONNECTION_STRING` | MongoDB connection string | `mongodb://user:pass@host:port/` |
| `ASPNETCORE_ENVIRONMENT` | Deployment environment | `Production`, `Development` |
| `ASPNETCORE_URLS` | API URL bindings | `http://+:8090` |
| `MONGODB_DATABASE_NAME` | MongoDB database name | `leaderboard` |
| `MONGODB_COLLECTION_NAME` | MongoDB collection name | `records` |

## API Endpoints

### Health Check
```
GET /Health
```
Returns the health status of the API.

### Submit Score
```
POST /api/score/submit
Content-Type: application/json

{
  "playerName": "string",
  "score": number,
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### Get Rankings
```
GET /api/score/rankings?limit=100
```
Returns the top scores from the leaderboard.

## Docker Deployment Details

### Multi-Stage Build

The `Dockerfile` uses a multi-stage build process for optimal image size:

1. **base**: ASP.NET Core runtime image
2. **build**: .NET SDK with project dependencies and compilation
3. **publish**: Publishes the release build
4. **final**: Production image with only the runtime

### Health Check Configuration

Docker Compose includes a health check that:
- Tests the `/Health` endpoint every 30 seconds
- Times out after 10 seconds
- Retries up to 3 times before marking as unhealthy
- Waits 40 seconds before starting the health check

### Restart Policy

- **Condition**: Restarts on any exit
- **Delay**: 5 seconds between restart attempts
- **Max Attempts**: 3 attempts within a 120-second window

## Development

### Project Structure

```
Leaderboard-API/
├── Controllers/          # API endpoints
│   ├── ScoreController.cs
│   └── HealthController.cs
├── Models/          # Data models
│   ├── Record.cs
│   ├── LeaderboardEntry.cs
│   ├── ScoreSubmissionRequest.cs
│   ├── ScoreSubmissionResponse.cs
│   └── GetRankingsResponse.cs
├── Services/       # Business logic
│   ├── IMongoDbService.cs
│   └── MongoDbService.cs
├── Program.cs           # Application entry point
├── appsettings.json     # Configuration
└── Leaderboard-API.csproj
```

### Dependencies

- **MongoDB.Driver** (v3.0.0): MongoDB database driver
- **DotNetEnv** (v3.1.1): Environment variable loading
- **Microsoft.VisualStudio.Azure.Containers.Tools.Targets**: Docker support

## Troubleshooting

### MongoDB Connection Issues

**Error**: `Unable to connect to MongoDB`

**Solution**:
1. Verify the `MONGO_CONNECTION_STRING` in `.env` is correct
2. Check that MongoDB is running and accessible
3. Ensure credentials have the necessary permissions
4. For MongoDB Atlas, whitelist the container's IP address

### Port Already in Use

**Error**: `bind: address already in use`

**Solution**:
```bash
# Find and stop the process using the port
docker ps
docker stop <container_id>

# Or change to a different port (see Configuration section)
```

### Container Won't Start

**Solution**:
```bash
# Check logs for errors
docker logs leaderboard-api

# Verify the image was built correctly
docker images | grep leaderboard-api

# Rebuild the image
docker-compose build --no-cache
```

## CORS Configuration

The API is configured to allow requests from `itch.zone` and its subdomains for secure cross-origin access. This can be modified in `Program.cs`:

```csharp
policy.SetIsOriginAllowed(origin =>
{
    var uri = new Uri(origin);
    return uri.Host == "itch.zone" || uri.Host.EndsWith(".itch.zone");
})
```

## License

Please refer to the LICENSE file in the repository for licensing information.

## Support & Contributions

For issues, bug reports, or feature requests, please open an issue on the [GitHub repository](https://github.com/beetron/Leaderboard-API).

Contributions are welcome! Please fork the repository and submit a pull request.

## About

This leaderboard system was built to power the global rankings for [Mini Miner](https://lazy-onigiri.itch.io/mini-miner), a browser-based mining game. It demonstrates a production-ready backend solution for managing competitive gameplay data with real-time leaderboard features.

## Related Resources

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [MongoDB .NET Driver](https://www.mongodb.com/docs/drivers/csharp/)
- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
