namespace SignalRHubs.Models
{
    public class PostFileViewModel
    {
        public  Guid Id { get; set; }
        public int? Index { get; set; }
        public string? Url { get; set; }
        public string? Type { get; set; }
        public int? PreviewW { get; set; }
        public int? PreviewH { get; set; }
    }
}
