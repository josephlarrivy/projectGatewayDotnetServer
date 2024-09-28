using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DotnetServer.Models;
using DotnetServer.Repositories;
using Npgsql;
using DotnetServer.Services;

namespace DotnetServer.Controllers
{
    [ApiController]
    [Route("users")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationRepository _authenticationRepository;

        // Inject the repository via the constructor
        public AuthenticationController(IAuthenticationRepository authenticationRepository)
        {
            _authenticationRepository = authenticationRepository;
        }

        // regusters a new user
        [HttpPost("registerNewUser")]
        public async Task<IActionResult> RegisterNewUser([FromBody] RegisterNewUserModel userModel)
        {
            try
            {
                // Attempt to register the user
                var result = await _authenticationRepository.RegisterNewUserAsync(userModel.Email, userModel.Password, userModel.FirstName, userModel.LastName);

                if (result.Success == false)
                {
                    return Conflict(new { Message = result.Message }); // Return 409
                }

                // Call the GenerateAndReturnLoginCode method from your repository
                var code = await _authenticationRepository.GenerateAndReturnLoginCodeAsync(userModel.Email, "register");

                if (code == null)
                {
                    return NotFound(new { Message = "User not found or unable to generate login code." });
                }

                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("checkLoginCode")]
        public async Task<IActionResult> CheckLoginCode([FromQuery] string code)
        {
            try
            {
                // Verify the login code
                bool isValid = await _authenticationRepository.VerifyLoginCodeAsync(code);

                if (isValid == true)
                {
                    Console.WriteLine("Code verified successfully");
                    return Ok(new { Message = "Code verified successfully", token = "xxx"  });
                }
                else
                {
                    // Using 403 Forbidden for invalid or expired code
                    return StatusCode(403, new { Message = "Invalid or expired code." });

                    // Alternatively, you could use 410 Gone
                    // return StatusCode(410, new { Message = "The provided code is no longer valid." });
                }
            }
            catch (NpgsqlException ex)
            {
                // Log database-related exceptions
                Console.WriteLine($"Database error occurred: {ex.Message}");
                return StatusCode(500, "Database error occurred.");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }





    }
}
