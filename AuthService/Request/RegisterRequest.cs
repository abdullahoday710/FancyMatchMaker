using System.ComponentModel.DataAnnotations;

namespace AuthService.Request
{
    public class RegisterRequest
    {
        [EmailAddress]
        public required string UserEmail { get; set; }
        public required string Password { get; set; }
        public required string UserName { get; set; }
    }
}
