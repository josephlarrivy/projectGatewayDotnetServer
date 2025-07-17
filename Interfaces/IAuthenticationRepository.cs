using DotnetServer.Models;

public interface IAuthenticationRepository
{
    Task<RegisterNewUserResultModel> RegisterNewUserAsync(string email, string password, string firstName, string lastName);
    Task<ReturnLoginCodeModel> GenerateAndReturnVerificationCodeAsync(string email, string codeType);
    Task<bool> VerifyVerificationCodeAsync(string email, string code);
    Task<AuthenticationResultModel> AuthenticateAsync(string email, string password);
}

