namespace SignalRHubs.Models
{
    public class SearchResultViewModel
    {
        public List<CommunityViewModel> communityViewModels { get; set; }
        public List<UserViewModel> usersViewModels { get; set; }
        public List<PostViewModel> postsViewModels { get; set; }
        public List<EventViewModel> eventsViewModels { get; set; }
        public SearchResultViewModel()
        {
            communityViewModels = new List<CommunityViewModel>();
            usersViewModels = new List<UserViewModel>();
            postsViewModels = new List<PostViewModel>();
            eventsViewModels = new List<EventViewModel>();
        }
    }
}
