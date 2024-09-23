using DotnetServer.Models;

public interface IAuthenticationRepository
{
    Task<UserModel> GetUserByIdAsync(int id);
    Task<ReturnLoginCodeModel> GenerateAndReturnLoginCode(string email);
}
