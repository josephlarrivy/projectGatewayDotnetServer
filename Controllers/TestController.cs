using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DotnetServer.Controllers
{
    [ApiController]
    [Route("test")]  // Base route for this controller
    public class TestController : ControllerBase
    {
        [HttpGet("testGateway")]  // Full route will be /api/test/gateway
        public Task<string> GetTest()
        {
            return Task.FromResult("Hitting Dotnet API via Gateway");
        }

    }
}
