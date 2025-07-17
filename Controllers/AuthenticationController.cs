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
        private readonly TokenGenerator _tokenGenerator;

        public AuthenticationController(IAuthenticationRepository authenticationRepository, TokenGenerator tokenGenerator)
        {
            _authenticationRepository = authenticationRepository;
            _tokenGenerator = tokenGenerator;
        }

        // registers a new user
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
                var code = await _authenticationRepository.GenerateAndReturnVerificationCodeAsync(userModel.Email, "register");

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

        //checks that login code matches, is not used, and is still valid
        [HttpGet("checkVerificationCode")]
        public async Task<IActionResult> CheckVerificationCode([FromQuery] string email, [FromQuery] string code)
        {
            try
            {
                // Verify the login code
                bool isValid = await _authenticationRepository.VerifyVerificationCodeAsync(email, code);

                if (isValid == true)
                {
                    // Console.WriteLine("Code verified successfully");
                    return Ok("Code verified successfully.");
                }
                else
                {
                    // Using 403 Forbidden for invalid or expired code
                    return StatusCode(403, new { Message = "Invalid or expired email and code combination." });

                }
            }
            catch (NpgsqlException ex)
            {
                // Log database-related exceptions
                // Console.WriteLine($"Database error occurred: {ex.Message}");
                return StatusCode(500, "Database error occurred.");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                // Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }





        // regusters a new user
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateModel authenticationData)
        {
            try
            {
                // Attempt to authenticate the user
                var result = await _authenticationRepository.AuthenticateAsync(
                    authenticationData.Email, authenticationData.Password
                );

                if (result.IsSuccess == false)
                {
                    return Unauthorized(
                        new { Message = "Invalid email or password." }
                    ); // Return 401
                }

                if (result.IsVerifiedByLoginCode == false)
                {
                    return Unauthorized(
                        new { Message = "User email address not yet verified." }
                    ); // Return 401
                }

                string token = _tokenGenerator.GenerateToken(
                    result.Id,
                    result.Email,
                    result.FirstName,
                    result.LastName
                );

                return Ok(new { token });

            }
            catch (Exception ex)
            {
                // Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }






    }
}
