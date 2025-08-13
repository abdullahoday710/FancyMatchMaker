using System.ComponentModel.DataAnnotations;

namespace MatchmakingService.Entities
{
    public class MatchMakingProfileEntity
    {
        [Key]
        public long ID { get; set; }
        public long UserID { get; set; }
        public int MatchesPlayed { get; set; }
        public int MatchesWon { get; set; }
        public int MatchesLost { get; set; }
    }
}
