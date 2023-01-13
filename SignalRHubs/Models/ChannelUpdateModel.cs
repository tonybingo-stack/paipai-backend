using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class ChannelUpdateModel
    {
        [Required]
        public Guid ChannelId { get; set; }
        public string? ChannelName { get; set; }
        public string? ChannelDescription { get; set; }
    }
}
