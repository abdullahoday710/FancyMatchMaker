namespace GameService.MatchState
{
    public class PlayerState
    {
        public required GameStances ChosenStance { get; set; }
        public required bool ChoiceMade { get; set; }
        public required int RoundsWon { get; set; }
    }
}
