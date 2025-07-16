using DotnetServer.Models;

public interface IAuthenticationRepository
{
    Task<RegisterNewUserResultModel> RegisterNewUserAsync(string email, string password, string firstName, string lastName);
    Task<ReturnLoginCodeModel> GenerateAndReturnLoginCodeAsync(string email, string codeType);
    Task<bool> VerifyLoginCodeAsync(string email, string code);
    Task<AuthenticationResultModel> AuthenticateAsync(string email, string password);
}

