namespace SignalRHubs.Models
{
    public class ReadUserSummaryModel
    {
        public Guid Id { get; set; }
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
    }
}
