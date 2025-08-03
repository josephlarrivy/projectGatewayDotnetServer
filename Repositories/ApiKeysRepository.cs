using DotnetServer.Models;
using Npgsql;
using Dapper;
using DotnetServer.Services;
using System.Security.Cryptography;
using System.Text;

namespace DotnetServer.Repositories
{
    public class ApiKeysRepository : IApiKeysRepository
    {
        private readonly string _connectionString;

        public ApiKeysRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Hash the API key using SHA256
        private string HashKeyWithSha256(string key)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(key);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        // Helper function to generate new API keys
        private async Task<(string rawKey, string hashedKey)> GenerateNewApiKeyAsync(string userId)
        {
            string GenerateRandomKey()
            {
                const string chars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ123456789";
                var bytes = new byte[20];
                var result = new char[20];

                RandomNumberGenerator.Fill(bytes);

                for (int i = 0; i < 20; i++)
                {
                    var idx = bytes[i] % chars.Length;
                    result[i] = chars[idx];
                }

                return new string(result);
            }

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            while (true)
            {
                string rawKey = $"api_{GenerateRandomKey()}_{userId}";
                string hashedKey = HashKeyWithSha256(rawKey);

                var sql = "SELECT 1 FROM ApiKeys WHERE HashedKey = @HashedKey";
                var existing = await connection.QuerySingleOrDefaultAsync<int?>(sql, new { HashedKey = hashedKey });

                if (existing == null)
                {
                    return (rawKey, hashedKey);
                }
            }
        }

        // Register and return new API key
        public async Task<GenerateNewApiKeyModel> RequestNewApiKey(string userId, string keyName)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var (rawKey, hashedKey) = await GenerateNewApiKeyAsync(userId);

                var sql = @"
                    INSERT INTO ApiKeys
                        (UserId, HashedKey, ExpiresAt, KeyName)
                    VALUES
                        (@UserId, @HashedKey, @ExpiresAt, @KeyName)";

                await connection.ExecuteAsync(sql, new
                {
                    UserId = userId,
                    HashedKey = hashedKey,
                    ExpiresAt = DateTime.UtcNow.AddDays(365),
                    KeyName = keyName
                });

                return new GenerateNewApiKeyModel
                {
                    Success = true,
                    Key = rawKey
                };
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                Console.WriteLine($"An error occurred: {ex.Message}");

                return new GenerateNewApiKeyModel
                {
                    Success = false
                };
            }
        }
    }
}
