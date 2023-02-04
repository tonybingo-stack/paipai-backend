using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class MessageBindingModel
    {
        [Required]
        public string ReceiverUserName { get; set; }
        public string? Content { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public int? FilePreviewW { get; set; }
        public int? FilePreviewH { get; set; }
        public Guid? RepliedTo { get; set; }
    }
}
