namespace SignalRHubs.Models
{
    public class ChatChannelViewModel
    {
        public Guid ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string ChannelDescription { get; set; }
        public Guid ChannelCommunityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
