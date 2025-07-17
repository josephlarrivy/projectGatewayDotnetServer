using DotnetServer.Models;
using Npgsql;
using Dapper;
using DotnetServer.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

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
                email = email.Trim().ToLower();
                var sql = "SELECT * FROM Users WHERE NormalizedEmail = @Email";
                var user = await connection.QuerySingleOrDefaultAsync<UserModel?>(sql, new { Email = email });
                return user;
            }
        }

        // Helper function to generate user ids
        private async Task<string> GenerateNewUserIdAsync()
        {
            string GenerateRandomUserId()
            {
                // var random = new Random();
                // const string chars = "23456789abcdefghijkmnpqrstuvwxyz";
                // return new string(Enumerable.Repeat(chars, 12)
                //     .Select(s => s[random.Next(s.Length)]).ToArray());

                const string chars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ123456789";
                var bytes = new byte[12];
                var result = new char[12];

                RandomNumberGenerator.Fill(bytes);

                for (int i = 0; i < 12; i++)
                {
                    var idx = bytes[i] % chars.Length;
                    result[i] = chars[idx];
                }

                return new string(result);
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                while (true)
                {
                    string newId = GenerateRandomUserId();
                    var sql = "SELECT 1 FROM Users WHERE id = @NewId";
                    var existing = await connection.QuerySingleOrDefaultAsync<int?>(sql, new { NewId = newId });

                    if (existing == null) // ID is unique
                    {
                        return newId;
                    }
                }
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
                    var newId = await GenerateNewUserIdAsync();

                    var sql = @"
                        INSERT INTO Users
                            (Id, Email, NormalizedEmail, HashedPassword, FirstName, LastName, CreatedAt)
                        VALUES
                            (@Id, @Email, @NormalizedEmail, @HashedPassword, @FirstName, @LastName, @CreatedAt)";

                    await connection.ExecuteAsync(sql, new
                    {
                        Id = newId,
                        Email = email,
                        NormalizedEmail = email.Trim().ToLower(),
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
        private string GenerateVerificationCode()
        {
            var bytes = RandomNumberGenerator.GetBytes(6);
            var digits = bytes.Select(b => (char)('0' + (b % 10)));
            return new string(digits.Take(6).ToArray());
        }

        // Method to generate login code given an email, this will me eamiled to a user from the express app and they will be ablee to us it to log in
        public async Task<ReturnLoginCodeModel?> GenerateAndReturnVerificationCodeAsync(string email, string codeType)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var loginCode = GenerateVerificationCode();
                var expiresAt = DateTime.Now.AddMinutes(5);
                var createdAt = DateTime.Now;

                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO VerificationCodes
                        (NormalizedEmail, Code, CodeType, ExpiresAt, CreatedAt)
                    VALUES 
                        (@NormalizedEmail, @Code, @CodeType, @ExpiresAt, @CreatedAt)
                    ";

                // Execute the Insert query using Dapper
                await connection.ExecuteAsync(sql, new
                {
                    NormalizedEmail = email.Trim().ToLower(),
                    Code = loginCode,
                    CodeType = codeType,
                    ExpiresAt = expiresAt,
                    CreatedAt = createdAt
                });

                _emailSender.SendLoginCodeEmail(email, loginCode);


                // Return the generated login code
                return new ReturnLoginCodeModel
                {
                    Email = email,
                    // LoginCode = loginCode
                };
            }
        }

        // verify the login code exists and is still valid
        public async Task<bool> VerifyVerificationCodeAsync(string email, string code)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Check if the code exists and has not expired
                    var sql = @"SELECT Id FROM VerificationCodes WHERE Code = @Code And NormalizedEmail = @NormalizedEmail AND ExpiresAt > NOW() AND IsUsed = FALSE";

                    // Log the SQL query and parameters for debugging
                    // Console.WriteLine($"Executing SQL: {sql} with parameters: Email={email} Code={code}");

                    var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Code = code, NormalizedEmail = email.Trim().ToLower() });

                    if (result != null)
                    {
                        // Code is valid; mark it as used
                        var setToUsedCodeSql = @"UPDATE VerificationCodes SET IsUsed = TRUE WHERE Code = @Code";
                        await connection.ExecuteAsync(setToUsedCodeSql, new { Code = code });
                        // Console.WriteLine($"Login code {code} is valid and marked as used.");

                        // Code is valid; mark user as verified
                        // var setToVerifiedSql = @"UPDATE Users SET IsVerifiedByLoginCode = TRUE WHERE Email = @Email";
                        // await connection.ExecuteAsync(setToVerifiedSql, new { Email = email });

                        var setToVerifiedSql = @"UPDATE Users SET IsVerifiedByLoginCode = TRUE WHERE NormalizedEmail = @NormalizedEmail";
                        await connection.ExecuteAsync(setToVerifiedSql, new { NormalizedEmail = email.Trim().ToLower() });
                        // Console.WriteLine($"User with email {email} marked as verified.");

                        return true;
                    }

                    // Console.WriteLine($"Login code {code} and {email} combination is not valid or has expired.");
                    return false;
                }
            }
            catch (NpgsqlException ex)
            {
                // Log database-related exceptions
                // Console.WriteLine($"Database error occurred: {ex.Message}");
                // Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
            catch (Exception ex)
            {
                // Log general exceptions
                // Console.WriteLine($"An error occurred: {ex.Message}");
                // Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }




        // Authenticate User
        public async Task<AuthenticationResultModel> AuthenticateAsync(string email, string password)
        {
            try
            {
                var existingUser = await GetUserByEmailAsync(email);
                if (existingUser == null)
                {
                    await RecordUnsuccessfulLoginAttempts(email, "User with submitted email address not found.");
                    return new AuthenticationResultModel { IsSuccess = false };
                }

                var dummyUser = new object();
                var result = _passwordHasher.VerifyHashedPassword(dummyUser, existingUser.HashedPassword, password);

                if (result == PasswordVerificationResult.Success)
                {

                    if (existingUser.IsVerifiedByLoginCode == false) {
                        await RecordUnsuccessfulLoginAttempts(email, "User has not yet verified their email address.");
                    }

                    return new AuthenticationResultModel
                    {
                        Id = existingUser.Id,
                        IsSuccess = true,
                        Email = existingUser.Email,
                        FirstName = existingUser.FirstName,
                        LastName = existingUser.LastName,
                        IsVerifiedByLoginCode = existingUser.IsVerifiedByLoginCode
                    };
                }

                await RecordUnsuccessfulLoginAttempts(email, "Submitted email and password do not match.");
                return new AuthenticationResultModel { IsSuccess = false };
            }
            catch (Exception ex)
            {
                return new AuthenticationResultModel { IsSuccess = false };
            }
        }


        // Method to record unsuccessful authentication requests and reasons why unsuccessful
        public async Task<bool> RecordUnsuccessfulLoginAttempts(string email, string reason)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO UnsuccessfulLoginAttempts
                        (NormalizedEmail, Reason)
                    VALUES
                        (@NormalizedEmail, @Reason)
                ";

                await connection.ExecuteAsync(sql, new
                {
                    NormalizedEmail = email.Trim().ToLower(),
                    Reason = reason
                });

                return true;
            }
        }






    }
}