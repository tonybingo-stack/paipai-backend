using Dapper.Contrib.Extensions;

namespace SignalRHubs.Entities
{
    [Table("ChatRoomDetails")]
    public class ChatRoomDetail : BaseEntity
    {
        [ExplicitKey]
        public override Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public Guid UserId { get; set; }

        public bool IsGroupChat { get; set; }

        [Write(false)]
        public ChatRoom ChatRoom { get; set; }

        public ChatRoomDetail()
        {
            Id = Guid.NewGuid();
        }
    }
}