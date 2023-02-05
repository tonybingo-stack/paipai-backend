using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class EditUserModel
    {
        [Required]
        public Guid Id { get; set; }
        public string? NickName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string Gender { get; set; }
        public string Avatar { get; set; }
        public string Background { get; set; }
        public string? UserBio { get; set; }
    }
}
