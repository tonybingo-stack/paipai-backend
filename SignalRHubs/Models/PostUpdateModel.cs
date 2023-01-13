using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class PostUpdateModel
    {
        [Required]
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Contents { get; set; }
        public string? Price { get; set; }
        public string? Category { get; set; }
    }
}
