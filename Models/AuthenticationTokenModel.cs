namespace DotnetServer.Models
{
    public class AuthenticationTokenModel
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
    }
}