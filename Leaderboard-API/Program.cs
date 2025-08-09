using DotNetEnv;
using Leaderboard_API.Services;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS for itch.io and subdomains only
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowItchIo", policy =>
    {
        policy.WithOrigins("https://itch.io", "https://*.itch.io")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register MongoDB service
builder.Services.AddSingleton<IMongoDbService, MongoDbService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Only use HTTPS redirection in Development (nginx handles SSL termination)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable CORS
app.UseCors("AllowItchIo");

app.UseAuthorization();

app.MapControllers();

app.Run();
