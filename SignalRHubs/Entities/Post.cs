using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SignalRHubs.Entities
{
    [Table("Posts")]
    public class Post:BaseEntity
    {
        public string UserName { get; set; }
        public Guid CommunityID { get; set; }
        public string Title { get; set; }
        public string? Contents { get; set; }
        public string? Price { get; set; }
        public string? Category { get; set; }
        public bool isDeleted { get; set; }

    }
}
