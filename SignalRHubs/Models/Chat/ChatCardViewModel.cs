namespace SignalRHubs.Models
{
    public class ChatCardViewModel
    {
        public string SenderUserName { get; set; }
        public string ReceiverUserName { get; set; }
        public string Content { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public bool isSend { get; set; }
        public string? NickName { get; set; }
        public string? Avatar { get; set; }
    }
}
