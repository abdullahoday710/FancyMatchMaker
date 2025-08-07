using System.ComponentModel.DataAnnotations;

namespace MatchmakingService.Entities
{
    public class MatchMakingProfileEntity
    {
        [Key]
        public long ID { get; set; }

        public long UserID { get; set; }
    }
}
