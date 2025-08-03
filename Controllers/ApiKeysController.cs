using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DotnetServer.Models;
using DotnetServer.Repositories;
using Npgsql;
using DotnetServer.Services;

namespace DotnetServer.Controllers
{
    [ApiController]
    [Route("keys")]
    public class ApiKeysController : ControllerBase
    {
        private readonly IApiKeysRepository _apiKeysRepository;

        public ApiKeysController(IApiKeysRepository apiKeysRepository)
        {
            _apiKeysRepository = apiKeysRepository;
        }

        // creates and returns a new api key
        [HttpPost("requestNewApiKey")]
        public async Task<IActionResult> RequestNewApiKey([FromBody] RequestNewApiKeyModel requestModel)
        {
            try
            {
                // Attempt to register the user
                var result = await _apiKeysRepository.RequestNewApiKey(requestModel.UserId, requestModel.KeyName);

                // if (result.Success == false)
                // {
                //     return Conflict(new { Message = result.Message });
                // }

                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


    }
}
