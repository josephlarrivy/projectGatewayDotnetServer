using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DotnetServer.Models;
using DotnetServer.Repositories;

namespace DotnetServer.Controllers
{
    [ApiController]
    [Route("demo")]
    public class DemoAuthController : ControllerBase
    {
        private readonly IAuthenticationRepository _authenticationRepository;

        // Inject the repository via the constructor
        public DemoAuthController(IAuthenticationRepository authenticationRepository)
        {
            _authenticationRepository = authenticationRepository;
        }

        [HttpGet("getDemoToken")]
        public Task<AuthenticationTokenModel> GetDemoToken()
        {
            // Creating a demo token
            var token = new AuthenticationTokenModel
            {
                Id = 1,
                Email = "demoUser@demo.com",
                Name = "demoUser",
            };

            // Returning the demo token
            return Task.FromResult(token);
        }

        // Add this action to call GetUserByIdAsync
        [HttpGet("getUser/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _authenticationRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }

        // demo setup to make Repo method route works
        // this is not an actual route and will be removed
        [HttpGet("testLoginCode/{email}")]
        public async Task<IActionResult> TestLoginCode(string email)
        {
            // Call the GenerateAndReturnLoginCode method from your repository
            var code = await _authenticationRepository.GenerateAndReturnLoginCode(email);

            // If the code is null (e.g., email not found), return a NotFound result
            if (code == null)
            {
                return NotFound(new { Message = "User not found or unable to generate login code." });
            }

            // Return the generated code as a JSON result
            return Ok(code);
        }

    }
}
