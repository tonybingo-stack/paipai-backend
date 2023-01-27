using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Interfaces.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHomeService
    {
        Task<Guid> CreateCommunity(Community entity);
        Task<Guid> UpdateCommunity(Community entity);
        Task<Guid> DeleteCommunity(Guid id);
        Task<Guid> CreateChannel(Channel entity);
        Task<IEnumerable<ChannelViewModel>> GetAllChannels(Guid communityID);
        Task<Guid> UpdateChannel(ChannelUpdateModel model);
        Task<Guid> DeleteChannel(Guid id);
        Task<String> UpdateCommunityAvatar(Guid id, string url);
        Task<String> UpdateCommunityBackGround(Guid id, string url);
        Task<Guid> CreatePost(Post entity, PostCreateModel model);
        Task<IEnumerable<PostViewModel>> GetPosts(string username);
        Task<IEnumerable<PostViewModel>> GetPostsForFeed(int offset);
        Task<SearchResultViewModel> GetSearchResult(string text);
        Task<Guid> UpdatePost(PostUpdateModel model);
        Task<Guid> DeletePost(Guid postId);
        Task<IEnumerable<CommunityViewModel>> GetJoinedCommunity(string username);
        Task<IEnumerable<CommunityViewModel>> GetCommunityForFeed(int offset);
        Task<string> JoinCommunity(string username,Guid communityId);
        Task<string> ExitCommunity(string username, Guid communityId);
        Task<CommunityMember> GetUserRole(string username, Guid communityId);
        Task<Channel> GetChannelById(Guid channelId);
        Task<Community> GetCommunityById(Guid id);
        Task<Guid> CreateEvent(Event e);
        Task<Event> GetEventByID(Guid id);
        Task<Guid> UpdateEvent(Event e);
        Task<Guid> DeleteEvent(Guid id);
        Task<IEnumerable<EventViewModel>> GetAllEvent(Guid communityId);
        Task<Community> GetCommunityByChannelId(Guid channelId);
        Task<string> AddAdmin(string username, Guid communityId);
        Task<string> RemoveAdmin(string username, Guid communityId);
        Task<string> JoinChannel(string name, Guid channelId);
        Task<string> ExitChannel(string name, Guid channelId);
        Task UpdateUserNumberOfCommunity(int v, Guid communityId);
        Task<string> LikePost(string userName, Guid postId);
        Task<string> UnLikePost(string userName, Guid postId);
    }
}
