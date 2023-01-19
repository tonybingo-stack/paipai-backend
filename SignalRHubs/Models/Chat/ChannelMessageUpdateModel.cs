using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class ChannelMessageUpdateModel
    {
        [Required]
        public Guid Id { get; set; }
        public string? Content { get; set; }
        public string? FilePath { get; set; }
    }
}
