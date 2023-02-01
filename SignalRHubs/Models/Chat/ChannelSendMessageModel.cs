using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class ChannelSendMessageModel
    {
        [Required]
        public Guid ChannelId { get; set; }
        public string? Content { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public int? FilePreviewW { get; set; }
        public int? FilePreviewH { get; set; }
        public Guid? RepiedTo { get; set; }
    }
}
