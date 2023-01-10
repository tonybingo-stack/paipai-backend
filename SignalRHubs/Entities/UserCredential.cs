using Dapper.Contrib.Extensions;
using DbHelper.Enums;
using DbHelper.Interfaces;

namespace SignalRHubs.Entities
{
    [Table("UserCredential")]

    public class UserCredential : BaseEntity
    {
        public override Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string NickName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int Gender { get; set; }
        public DateTime RegisterTime { get; set; }
        public string Avatar { get; set; }

        [Write(false)]
        public EntityState EntityState { get; set; }
    }
}
