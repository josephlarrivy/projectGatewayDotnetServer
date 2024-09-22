using DotnetServer.Models;

public interface IAuthenticationRepository
{
    Task<UserModel> GetUserByIdAsync(int id);
}
