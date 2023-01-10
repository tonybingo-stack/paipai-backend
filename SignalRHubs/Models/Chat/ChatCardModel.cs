namespace SignalRHubs.Models
{
    public class ChatCardModel
    {
        public Guid UserID { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; }
        public bool isSend { get; set; }
        public bool isDeleted { get; set; }
        public string NickName { get; set; }
        public string Avatar { get; set; }
    }
}
