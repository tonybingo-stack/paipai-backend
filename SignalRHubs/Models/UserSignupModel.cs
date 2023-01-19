using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class UserSignupModel
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string NickName { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }
        public int Gender { get; set; }
        public DateTime RegisterTime { get; set; }
        public string Avatar { get; set; }
        public string Background { get; set; }
        public string Token { get; set; }
    }
}
