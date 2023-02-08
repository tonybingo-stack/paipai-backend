using Dapper.Contrib.Extensions;

namespace SignalRHubs.Entities
{
    [Table("ChannelMessage")]
    public class ChannelMessage : BaseEntity
    {
        public override Guid Id { get; set; }
        public string SenderUserName { get; set; }
        public string? Content { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public int? FilePreviewW { get; set; }
        public int? FilePreviewH { get; set; }
        public Guid ChannelId { get; set; }
        public bool? isDeleted { get; set; }
        public Guid? RepliedTo { get; set; }
    }
}
