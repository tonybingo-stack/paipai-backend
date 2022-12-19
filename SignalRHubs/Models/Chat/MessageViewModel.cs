using SignalRHubs.Entities;

namespace SignalRHubs.Models
{
    public class MessageViewModel : BaseEntity
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; }
        public Guid? RoomId { get; set; }
        public string FilePath { get; set; }
    }
}
