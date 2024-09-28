using DotnetServer.Models;
using Npgsql;
using Dapper;
using DotnetServer.Services;
using Microsoft.AspNetCore.Identity;

namespace DotnetServer.Repositories
{
    public class DemoAuthRepository : IDemoAuthRepository
    {
        private readonly string _connectionString;
        private readonly EmailSender _emailSender;
        private readonly PasswordHasher<object> _passwordHasher; // Generic PasswordHasher with object

        public DemoAuthRepository(string connectionString, EmailSender emailSender)
        {
            _connectionString = connectionString;
            _emailSender = emailSender;
            _passwordHasher = new PasswordHasher<object>(); // Initialize PasswordHasher with object
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
            // const string chars = "abcdefghijklmnopqrstuvwxyz123456789";
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Method to generate login code given an email, this will me eamiled to a user from the express app and they will be ablee to us it to log in
        public async Task<ReturnLoginCodeModel?> GenerateAndReturnLoginCodeAsync(string email)
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

        // Register new user with hashed password
        public async Task<RegisterNewUserResultModel> RegisterNewUserAsync(string email, string password, string firstName, string lastName)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var createdAt = DateTime.Now;

                    // Create a dummy user object to use with the PasswordHasher
                    var dummyUser = new object();

                    // Hash the password using PasswordHasher<object>
                    var hashedPassword = _passwordHasher.HashPassword(dummyUser, password);

                    var sql = @"INSERT INTO Users (Email, HashedPassword, FirstName, LastName, CreatedAt)
                        VALUES (@Email, @HashedPassword, @FirstName, @LastName, @CreatedAt)";

                    await connection.ExecuteAsync(sql, new
                    {
                        Email = email,
                        HashedPassword = hashedPassword,
                        FirstName = firstName,
                        LastName = lastName,
                        CreatedAt = createdAt
                    });

                    return new RegisterNewUserResultModel
                    {
                        Success = true,
                        Message = "User registered successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                Console.WriteLine($"An error occurred: {ex.Message}");

                return new RegisterNewUserResultModel
                {
                    Success = false,
                    Message = "Failed to register the user. Please try again later."
                };
            }
        }


    }
}