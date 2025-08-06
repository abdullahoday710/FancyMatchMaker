using System.ComponentModel.DataAnnotations;

namespace AuthService.Entities
{
    public class UserEntity
    {
        [Key]
        public long ID { get; set; }

        public required string UserEmail { get; set; }
    }
}
