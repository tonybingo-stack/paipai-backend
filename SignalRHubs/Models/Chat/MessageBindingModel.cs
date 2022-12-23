using Microsoft.AspNetCore.Http;

namespace SignalRHubs.Models
{
    public class MessageBindingModel
    {
        public Guid ReceiverId { get; set; }
        public string Content { get; set; }
        public Guid? ChannelId { get; set; }
        public IFormFile? File { get; set; }
    }
}
