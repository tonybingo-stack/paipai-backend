namespace SignalRHubs.Models
{
    public class CommunityMemberViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string NickName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string? Gender { get; set; }
        public DateTime RegisterTime { get; set; }
        public string? Avatar { get; set; }
        public string? Background { get; set; }
        public string? UserBio { get; set; }
        public int UserRole { get; set; }
    }
}
