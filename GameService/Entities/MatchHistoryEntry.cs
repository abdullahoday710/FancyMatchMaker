using System.ComponentModel.DataAnnotations;

namespace GameService.Entities
{
    public class MatchHistoryEntry
    {
        [Key]
        public long ID { get; set; }
        public required List<long> ParticipatingPlayers { get; set; }
        public long? WinnerID { get; set; }
        public required string MatchUUID { get; set; }
    }
}
