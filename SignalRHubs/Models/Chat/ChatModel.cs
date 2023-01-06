namespace SignalRHubs.Models
{
    public class ChatModel
    {
        public Guid SenderID { get; set; }
        public string Content { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}
