namespace SignalRHubs.Models
{
    public class CommunityUpdateModel
    {
        public Guid Id { get; set; }
        public string? CommunityName { get; set; }
        public string? CommunityDescription { get; set; }
        public string? CommunityType { get; set; }
        public string? ForegroundImage { get; set; }
        public string? BackgroundImage { get; set; }
    }
}
