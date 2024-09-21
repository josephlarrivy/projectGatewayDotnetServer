using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DotnetServer.Models;

namespace DotnetServer.Controllers
{
    [ApiController]
    [Route("demo")]
    public class DemoAuthController : ControllerBase
    {
        [HttpGet("getDemoToken")]
        public Task<AuthenticationTokenModel> GetDemoToken()
        {
            // Creating a demo token
            var token = new AuthenticationTokenModel
            {
                Id = 1,
                Username = "demoUser",
                UserId = 9999
            };

            // Returning the demo token
            return Task.FromResult(token);
        }
    }
}
