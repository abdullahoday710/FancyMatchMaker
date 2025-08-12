namespace Common.Messaging
{
    public class NewMatchStartedMessage
    {
        public required string matchID { get; set; }
        public required List<long> playerIDs { get; set; }
    }
}
