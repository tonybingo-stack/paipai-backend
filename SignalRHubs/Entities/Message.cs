using Dapper.Contrib.Extensions;

namespace SignalRHubs.Entities
{
    [Table("Messages")]
    public class Message : BaseEntity
    {

        [ExplicitKey]
        public override Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; }
        public Guid RoomId { get; set; }

        /// <summary>
        /// Store the file azure and store your file path when necessary.
        /// </summary>
        public string? FilePath { get; set; }

        [Write(false)]
        public virtual ChatRoom Room { get; set; }
    }
}