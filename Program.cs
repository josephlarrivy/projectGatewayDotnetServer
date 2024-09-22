using DotnetServer.Models;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Dapper;
using DotNetEnv;
using DotnetServer.Repositories;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var host = Environment.GetEnvironmentVariable("DATABASE_HOST");
var dbName = Environment.GetEnvironmentVariable("DATABASE_NAME");
var user = Environment.GetEnvironmentVariable("DATABASE_USER");
var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");

var connectionString = $"Host={host};Database={dbName};Username={user};Password={password}";
builder.Services.AddScoped<IAuthenticationRepository>(provider => new AuthenticationRepository(connectionString));


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
