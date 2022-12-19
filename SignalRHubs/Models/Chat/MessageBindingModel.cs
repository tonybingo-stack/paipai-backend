using Microsoft.AspNetCore.Http;

namespace SignalRHubs.Models
{
    public class MessageBindingModel
    {
        public Guid ReceiverId { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Room Id 
        /// <list type="bullet">
        /// <item>Null when new chat is starting.</item>
        /// <item>RoomId for existing chat.</item>
        /// </list>
        /// </summary>
        public Guid? RoomId { get; set; }
        public IFormFile? File { get; set; }
    }
}
