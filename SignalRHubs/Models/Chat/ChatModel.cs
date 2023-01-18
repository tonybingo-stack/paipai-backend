﻿namespace SignalRHubs.Models
{
    public class ChatModel
    {
        public string SenderUserName { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? RepliedTo { get; set; }
        public string? RepliedUserName { get; set; }
        public string? RepliedContent { get; set; }
        public DateTime? RepliedMsgCreatedAt { get; set; }
    }
}
