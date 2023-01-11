namespace SignalRHubs.Models
{
    public class MessageUpdateModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string? FilePath { get; set; }
    }
}
