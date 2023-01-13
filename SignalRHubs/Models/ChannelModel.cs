using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class ChannelModel
    {
        [Required]
        public string ChannelName { get; set; }
        public string? ChannelDescription { get; set; }
        [Required]
        public Guid ChannelCommunityId { get; set; }
    }
}
