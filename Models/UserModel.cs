namespace DotnetServer.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}