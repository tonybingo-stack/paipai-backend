using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class UserSigninModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
