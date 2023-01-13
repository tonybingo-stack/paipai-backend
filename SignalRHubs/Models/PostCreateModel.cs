using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class PostCreateModel
    {
        [Required]
        public Guid CommunityID { get; set; }
        [Required]
        public string Title { get; set; }
        public string? Contents { get; set; }
        public string? Price { get; set; }
        public string? Category { get; set; }
    }
}
