namespace SignalRHubs.Models
{
    public class ChannelViewModel
    {
        public Guid ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string ChannelDescription { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
