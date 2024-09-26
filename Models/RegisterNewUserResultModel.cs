namespace DotnetServer.Models
{
    public class RegisterNewUserResultModel
    {
        public bool? Success { get; set; }
        public string? Message { get; set; }
        public string? Email { get; set; }
    }
}