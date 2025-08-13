namespace AuthService.Response
{
    public class LoginResponse
    {
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required long UserId { get; set; }
        public required string AuthToken { get; set; }
    }
}
