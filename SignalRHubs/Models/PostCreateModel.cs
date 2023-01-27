using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class PostCreateModel
    {
        [Required]
        public string Title { get; set; }
        public string? Contents { get; set; }
        public string? Price { get; set; }
        public string? Category { get; set; }
        public List<Guid>? CommunityIds { get; set; }
        public List<string>? Urls { get; set; }
        public List<string>? Types { get; set; }
    }
}
