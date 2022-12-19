using Dapper.Contrib.Extensions;

namespace SignalRHubs.Entities
{
    [Table("Channel")]
    public class Channel : BaseEntity
    {
        [ExplicitKey]

        public Guid ChannelId { get; set; }

        public string ChannelName { get; set; }

        public string ChannelDescription { get; set; }
        public Guid ChannelCommunityId { get; set; }

        public Channel()
        {
            ChannelId = Guid.NewGuid();
        }
    }
}
