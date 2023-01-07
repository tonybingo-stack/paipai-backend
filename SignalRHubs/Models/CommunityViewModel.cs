namespace SignalRHubs.Models
{
    public class CommunityViewModel
    {
        public Guid Id { get; set; }
        public string CommunityName { get; set; }
        public string CommunityDescription { get; set; }
        public string CommunityType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ForegroundImage { get; set; }
        public string BackgroundImage { get; set; }

    }
}
