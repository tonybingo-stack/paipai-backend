using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class CreateUserModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string NickName { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Avatar { get; set; }
        public string? Background { get; set; }
        public string? UserBio { get; set; }
    }
}
