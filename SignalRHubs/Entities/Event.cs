using Dapper.Contrib.Extensions;

namespace SignalRHubs.Entities
{
    [Table("Event")]
    public class Event:BaseEntity
    {
        public string Title { get; set; }
        public string HostName { get; set; }
        public string Description { get; set; }
        public DateTime? Time { get; set; }
        public string Access { get; set; }
        public string Image { get; set; }
        public int Limit { get; set; }
        public Guid CommunityId { get; set; }
        public bool isDeleted { get; set; }
    }
}
