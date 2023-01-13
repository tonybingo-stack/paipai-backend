using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class MessageUpdateModel
    {
        [Required]
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string? FilePath { get; set; }
    }
}
