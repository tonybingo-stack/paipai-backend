using Dapper.Contrib.Extensions;
using DbHelper.Enums;
using DbHelper.Interfaces;

namespace SignalRHubs.Entities
{
    [Table("CommunityMember")]
    public class CommunityMember : IBaseEntity
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public Guid CommunityId { get; set; }
        public int UserRole { get; set; }
        public EntityState EntityState { get; set; }
    }
}
