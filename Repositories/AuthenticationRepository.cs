using DotnetServer.Models;
using Npgsql;
using Dapper;

namespace DotnetServer.Repositories;
public class AuthenticationRepository : IAuthenticationRepository
{
    private readonly string _connectionString;

    public AuthenticationRepository(string connectionString)
    {
        _connectionString = connectionString;
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
}
