using Dapper.Contrib.Extensions;
using DbHelper.Enums;
using DbHelper.Interfaces;

namespace SignalRHubs.Entities
{
    public class BaseEntity : IBaseEntity
    {
        public virtual Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Write(false)]
        public EntityState EntityState { get; set; }

        public BaseEntity()
        {
            EntityState = EntityState.Added;
            CreatedAt = DateTime.Now;
            Id = Guid.NewGuid();
        }
    }
}
