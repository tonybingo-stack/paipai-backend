namespace SignalRHubs.Models
{
    public class ChatRoomViewModel
    {
        public Guid RoomId { get; set; }
        public Guid ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public string PhotoUrl { get; set; }
    }
}
