using DotnetServer.Models;
using Npgsql;
using Dapper;
using DotnetServer.Services;

namespace DotnetServer.Repositories;
public class AuthenticationRepository : IAuthenticationRepository
{
    private readonly string _connectionString;
    private EmailSender _emailSender;

    public AuthenticationRepository(string connectionString, EmailSender emailSender)
    {
        _connectionString = connectionString;
        _emailSender = emailSender;
    }

    public async Task<UserModel?> GetUserByIdAsync(int id)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var sql = "SELECT * FROM Users WHERE Id = @Id";
            var user = await connection.QuerySingleOrDefaultAsync<UserModel?>(sql, new { Id = id });
            return user;
        }
    }


    // Helper function to generate a random code
    private string GenerateRandomCode()
    {
        var random = new Random();
        const string chars = "abcdefghijklmnopqrstuvwxyz123456789";
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    // Method to generate login code given an email, this will me eamiled to a user from the express app and they will be ablee to us it to log in
    public async Task<ReturnLoginCodeModel?> GenerateAndReturnLoginCode(string email)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var loginCode = GenerateRandomCode(); 
            var expiresAt = DateTime.Now.AddMinutes(5); 
            var createdAt = DateTime.Now; 

            await connection.OpenAsync();

            var sql = @"INSERT INTO LoginCodes (Email, Code, ExpiresAt, CreatedAt)
                    VALUES (@Email, @Code, @ExpiresAt, @CreatedAt)";

            // Execute the Insert query using Dapper
            await connection.ExecuteAsync(sql, new
            {
                Email = email,
                Code = loginCode,
                ExpiresAt = expiresAt,
                CreatedAt = createdAt
            });

            _emailSender.SendEmail(email, loginCode);


            // Return the generated login code
            return new ReturnLoginCodeModel
            {
                Email = email,
                // LoginCode = loginCode
            };
        }
    }

}
