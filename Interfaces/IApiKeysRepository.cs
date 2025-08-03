using DotnetServer.Models;

public interface IApiKeysRepository
{
    Task<GenerateNewApiKeyModel> RequestNewApiKey(string userId, string keyName);
    
}

