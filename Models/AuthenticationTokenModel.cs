namespace DotnetServer.Models
{
    public class AuthenticationTokenModel
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public int? UserId { get; set; }
    }
}