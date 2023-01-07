using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Interfaces.Services
{
    public interface IUserService
    {
        Task<Guid> GetIdByUserName(string name);
        Task<string> CreateUser(UserCredential entity);
        Task<string> LoginUser(LoginCredential entity);
        Task<User> GetUserByID(Guid userId);
        Task<User> GetUserByUserName(string name);
        Task<List<User>> GetUsers();
    }
}
