using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class MessageBindingModel
    {
        [Required]
        public string ReceiverUserName { get; set; }
        public string Content { get; set; }
        public Guid? ChannelId { get; set; }
        public string? FilePath { get; set; }
    }
}
