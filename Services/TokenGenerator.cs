using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
// using DotnetServer.Models;

namespace DotnetServer.Services
{
    public class TokenGenerator
    {
        private readonly string _secretKey;
        private readonly int _jwtLifespan; // Token lifespan in minutes

        public TokenGenerator(string jwtSecret, int jwtLifespan)
        {
            _secretKey = jwtSecret;
            _jwtLifespan = jwtLifespan;
        }

        public string GenerateToken(int id, string email, string firstName, string lastName)
        {
            // Define the token handler
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            // Create claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.GivenName, $"{firstName} {lastName}"),
            };

            // Define token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtLifespan),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Create token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Return the generated token
            return tokenHandler.WriteToken(token);
        }
    }
}
