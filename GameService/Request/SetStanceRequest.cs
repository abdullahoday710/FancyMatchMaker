using GameService.MatchState;

namespace GameService.Request
{
    public class SetStanceRequest
    {
        public required GameStances Stance { get; set; }
    }
}
