using LogikalMiddleware.Core.Models;
using LogikalMiddleware.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<LogikalSettings>(builder.Configuration.GetSection("Logikal"));

// Core services
builder.Services.AddSingleton<ILogikalService, LogikalService>();
builder.Services.AddSingleton<IFileDataService, FileDataService>();
builder.Services.AddHostedService<LogikalConnectionHostedService>();

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
var allowedOrigins = builder.Configuration.GetSection("Api:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

// Serve React dashboard from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// SPA fallback - serve index.html for non-API routes
app.MapFallbackToFile("index.html");

var port = builder.Configuration.GetValue<int>("Api:Port", 5000);
app.Urls.Clear();
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
