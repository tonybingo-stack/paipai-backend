namespace SignalRHubs.Models
{
    public class ChatHistoryModel
    {
        public Guid ID { get; set; }
        public Guid UserID { get; set; }
        public Guid MessageID { get; set; }
    }
}
