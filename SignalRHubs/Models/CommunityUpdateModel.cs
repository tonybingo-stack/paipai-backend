using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class CommunityUpdateModel
    {
        [Required]
        public Guid Id { get; set; }
        public string? CommunityName { get; set; }
        public string? CommunityDescription { get; set; }
        public string? CommunityType { get; set; }
        public string? ForegroundImage { get; set; }
        public string? BackgroundImage { get; set; }
        public int? NumberOfUsers { get; set; }
        public int? NumberOfPosts { get; set; }
        public int? NumberOfActiveUsers { get; set; }
    }
}
