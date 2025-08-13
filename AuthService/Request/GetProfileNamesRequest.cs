namespace AuthService.Request
{
    public class GetProfileNamesRequest
    {
        public required List<long> userIDs { get; set; }
    }
}
