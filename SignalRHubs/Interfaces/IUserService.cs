using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Interfaces.Services
{
    public interface IUserService
    {
        Task<Guid> GetIdByUserName(string name);
        Task<string> CreateUser(User entity);
        Task<string> UpdateUserAvatar(string url, string username);
        Task<string> LoginUser(UserModel model);
        Task<User> GetUserByUserName(string name);
        Task<List<User>> GetUsers();
        Task<bool> IsValidUserName(string userName);
    }
}
