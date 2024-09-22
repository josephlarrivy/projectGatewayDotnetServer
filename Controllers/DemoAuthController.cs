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
    }
}
