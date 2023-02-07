using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Interfaces.Services
{
    public interface IUserService
    {
        Task<Guid> GetIdByUserName(string name);
        Task<string> CreateUser(User entity);
        Task<string> UpdateUserAvatar(string url, string username);
        Task<string> LoginUser(UserSigninModel model);
        Task<User> GetUserByUserName(string name);
        Task<List<User>> GetUsers();
        Task<bool> IsValidUserName(string userName);
        Task<string> UpdateUserBackground(string url, string userName);
        Task<string> AcceptUserInvite(string userName, string username);
        Task<string> SendInvitation(string userName, string username);
        Task<List<FriendViewModel>> GetAllFriends(string userName);
        Task<string> RemoveUserFromFriend(string userName, string username);
        Task<string> BlockUser(string userName, string username);
        Task<string> EditUserProfile(string username, EditUserModel model);
    }
}
