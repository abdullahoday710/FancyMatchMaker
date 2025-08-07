namespace Common.Messaging
{
    public class UserRegisteredMessage
    {
        public required long UserID { get; set; }
        public required string UserName { get; set; }
        public required string UserEmail { get; set; }
    }
}
