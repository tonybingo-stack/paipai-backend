using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class EventUpdateModel
    {
        [Required]
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Time { get; set; }
        public string? Access { get; set; }
        public string? Image { get; set; }
        public int? Limit { get; set; }
    }
}
