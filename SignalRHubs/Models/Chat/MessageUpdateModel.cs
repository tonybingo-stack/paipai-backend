using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class MessageUpdateModel
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string ReceiverUserName { get; set; }
        public string? Content { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public int? FilePreviewW { get; set; }
        public int? FilePreviewH { get; set; }
    }
}
