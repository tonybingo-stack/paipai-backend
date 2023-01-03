namespace SignalRHubs.Models
{
    public class MessageUpdateModel
    {
        public string Content { get; set; }
        public IFormFile? File { get; set; }
    }
}
