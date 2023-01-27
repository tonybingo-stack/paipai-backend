using Dapper.Contrib.Extensions;

namespace SignalRHubs.Entities
{
    [Table("PostFile")]
    public class PostFile:BaseEntity
    {
        public override Guid Id { get; set; }
        public Guid PostId { get; set; }
        public int? Index { get; set; }
        public string? Url { get; set; }
        public string? Type { get; set; }
    }
}
