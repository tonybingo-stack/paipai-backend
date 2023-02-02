namespace SignalRHubs.Models
{
    public class FriendListModel
    {
        public Guid Id { get; set; }
        public string UserOne { get; set; }
        public string UserTwo { get; set; }
        public bool isBlocked { get; set; }
    }
}
