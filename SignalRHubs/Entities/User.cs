using Dapper.Contrib.Extensions;

namespace SignalRHubs.Entities
{
    [Table("Users")]
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Active { get; set; }

        public User()
        {
            this.Active = true;
        }
    }
}
