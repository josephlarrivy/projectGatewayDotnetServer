using DotnetServer.Models;
using Npgsql;
using Dapper;
using DotnetServer.Services;
using Microsoft.AspNetCore.Identity;

namespace DotnetServer.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly string _connectionString;
        private readonly EmailSender _emailSender;
        private readonly PasswordHasher<object> _passwordHasher; // Generic PasswordHasher with object

        public AuthenticationRepository(string connectionString, EmailSender emailSender)
        {
            _connectionString = connectionString;
            _emailSender = emailSender;
            _passwordHasher = new PasswordHasher<object>(); // Initialize PasswordHasher with object
        }

        public async Task<UserModel?> GetUserByEmailAsync(string email)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "SELECT * FROM Users WHERE Email = @Email";
                var user = await connection.QuerySingleOrDefaultAsync<UserModel?>(sql, new { Email = email });
                return user;
            }
        }

        // Register new user with hashed password
        public async Task<RegisterNewUserResultModel> RegisterNewUserAsync(string email, string password, string firstName, string lastName)
        {
            // Check if the email already exists
            var existingUser = await GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                // Email is already taken, return a conflict response
                return new RegisterNewUserResultModel
                {
                    Success = false,
                    Message = "User already registered"
                };
            }

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
        public async Task<ReturnLoginCodeModel?> GenerateAndReturnLoginCodeAsync(string email, string codeType)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var loginCode = GenerateRandomCode();
                var expiresAt = DateTime.Now.AddMinutes(5);
                var createdAt = DateTime.Now;

                await connection.OpenAsync();

                var sql = @"INSERT INTO LoginCodes (Email, Code, CodeType, ExpiresAt, CreatedAt)
                    VALUES (@Email, @Code, @CodeType, @ExpiresAt, @CreatedAt)";

                // Execute the Insert query using Dapper
                await connection.ExecuteAsync(sql, new
                {
                    Email = email,
                    Code = loginCode,
                    CodeType = codeType,
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

        // verify the login code exists and is still valid
        public async Task<bool> VerifyLoginCodeAsync(string code)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Check if the code exists and has not expired
                    var sql = @"SELECT Id FROM LoginCodes WHERE Code = @Code AND ExpiresAt > NOW() AND IsUsed = FALSE";

                    // Log the SQL query and parameters for debugging
                    Console.WriteLine($"Executing SQL: {sql} with parameters: Code={code}");

                    var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Code = code });

                    if (result != null)
                    {
                        // Code is valid; mark it as used
                        var setToUsedCodeSql = @"UPDATE LoginCodes SET IsUsed = TRUE WHERE Code = @Code";
                        await connection.ExecuteAsync(setToUsedCodeSql, new { Code = code });

                        Console.WriteLine($"Login code {code} is valid and marked as used.");
                        return true;
                    }

                    Console.WriteLine($"Login code {code} is not valid or has expired.");
                    return false;
                }
            }
            catch (NpgsqlException ex)
            {
                // Log database-related exceptions
                Console.WriteLine($"Database error occurred: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }




    }
}