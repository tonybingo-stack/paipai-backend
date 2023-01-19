using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class ChannelSendMessageModel
    {
        [Required]
        public Guid ChannelId { get; set; }
        public string? Content { get; set; }
        public string? FilePath { get; set; }
        public Guid? RepiedTo { get; set; }
    }
}
