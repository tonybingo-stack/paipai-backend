namespace SignalRHubs.Models
{
    public class ChannelMessageViewModel
    {
        public Guid Id { get; set; }
        public string SenderUserName { get; set; }
        public string Content { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public int? FilePreviewW { get; set; }
        public int? FilePreviewH { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? RepliedTo { get; set; }
        public string? RepliedUserName { get; set; }
        public string? RepliedUserAvatar { get; set; }
        public string? RepliedContent { get; set; }
        public DateTime? RepliedMsgCreatedAt { get; set; }
        public DateTime? RepliedMsgUpdatedAt { get; set; }

    }
}
