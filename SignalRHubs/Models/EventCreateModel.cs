using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class EventCreateModel
    {
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Time { get; set; }
        public string? Access { get; set; }
        public string? Image { get; set; }
        public int? Limit { get; set; }
        [Required]
        public Guid CommunityId { get; set; }
    }
}
