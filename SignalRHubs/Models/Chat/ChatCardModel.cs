namespace SignalRHubs.Models
{
    public class ChatCardModel
    {
        public Guid senderId { get; set; }
        public Guid receiverId { get; set; }
        public string content { get; set; }
    }
}
