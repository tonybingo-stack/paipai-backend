
namespace SignalRHubs.Models
{
    public class MessageViewModel
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; }
        public Guid? ChannelId { get; set; }
        public string FilePath { get; set; }
    }
}
