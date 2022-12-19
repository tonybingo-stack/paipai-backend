using Dapper.Contrib.Extensions;

namespace SignalRHubs.Entities
{
    [Table("ChatRooms")]
    public class ChatRoom : BaseEntity
    {   
        [ExplicitKey]
        public override Guid Id { get; set; }
        public string Name { get; set; }

        public bool IsGroupChat { get; set; }

        [Write(false)]
        public IEnumerable<Message> Messages { get; set; }
        [Write(false)]
        public IEnumerable<ChatRoomDetail> Details { get; set; }
    }
}