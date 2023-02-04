namespace SignalRHubs.Entities
{
    public class ChatCard:BaseEntity
    {
        public string SenderUserName { get; set; }
        public string ReceiverUserName { get; set; }
        public string? Content { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public bool isSend { get; set; }
    }
}
