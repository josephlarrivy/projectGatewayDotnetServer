using DotnetServer.Models;

public interface IDemoAuthRepository
{
    Task<UserModel> GetUserByIdAsync(int id);
    Task<ReturnLoginCodeModel> GenerateAndReturnLoginCodeAsync(string email);
    Task<RegisterNewUserResultModel> RegisterNewUserAsync(string email, string password, string firstName, string lastName);
}
