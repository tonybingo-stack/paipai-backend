using Dapper.Contrib.Extensions;
using DbHelper.Enums;
using DbHelper.Interfaces;

namespace SignalRHubs.Entities
{
    [Table("UserCredential")]

    public class LoginCredential : IBaseEntity
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        [Write(false)]
        public EntityState EntityState { get; set; }
    }
}
