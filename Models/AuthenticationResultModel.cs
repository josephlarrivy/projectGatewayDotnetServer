namespace DotnetServer.Models
{
    public class AuthenticationResultModel
    {
        public bool? IsSuccess { get; set; }
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? IsVerifiedByLoginCode { get; set; }
    }
}