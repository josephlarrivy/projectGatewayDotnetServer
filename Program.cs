using DotnetServer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Npgsql;
using Dapper;
using DotNetEnv;
using DotnetServer.Repositories;
using DotnetServer.Services;

// Load the environment to get variables
Env.Load();

// Fetch environment variables for database configuration
var host = Environment.GetEnvironmentVariable("DATABASE_HOST");
var dbName = Environment.GetEnvironmentVariable("DATABASE_NAME");
var dbUser = Environment.GetEnvironmentVariable("DATABASE_USER");
var dbPassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");

// Fetch environment variables for SMTP Gmail sending configuration
var smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT"));
var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
var smtpFromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL");

var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register EmailSender and pass configuration from .env
builder.Services.AddScoped(provider =>
    new EmailSender(smtpServer, smtpPort, smtpUser, smtpPassword, smtpFromEmail, frontendUrl));

// Register the AuthenticationRepository and inject the EmailSender
var connectionString = $"Host={host};Database={dbName};Username={dbUser};Password={dbPassword}";

builder.Services.AddScoped<IDemoAuthRepository>(provider =>
    new DemoAuthRepository(connectionString, provider.GetRequiredService<EmailSender>()));
builder.Services.AddScoped<IAuthenticationRepository>(provider =>
    new AuthenticationRepository(connectionString, provider.GetRequiredService<EmailSender>()));

var app = builder.Build();






// Use custom middleware for logging requests
app.UseMiddleware<DotnetServer.Middleware.RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

// Map controllers to routes
app.MapControllers();

app.Run();