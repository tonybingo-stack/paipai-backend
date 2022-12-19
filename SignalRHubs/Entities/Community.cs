using Dapper.Contrib.Extensions;

namespace SignalRHubs.Entities
{
    [Table("Community")]
    public class Community : BaseEntity
    {
        [ExplicitKey]

        public Guid CommunityId { get; set; }

        public string CommunityName { get; set; }

        public string CommunityDescription { get; set; }
        public Guid CommunityOwnerId { get; set; }
        public string CommunityType { get; set; }

        public Community()
        {
            CommunityId = Guid.NewGuid();
        }
    }
}
