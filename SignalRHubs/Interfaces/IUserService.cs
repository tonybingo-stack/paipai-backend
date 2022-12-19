using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Interfaces.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUserService
    {
        string Authenticate(UserModel entity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<string> CreateUser(UserCredential entity);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<string> LoginUser(LoginCredential entity);

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<User> GetUser(Guid userId);

        /// <summary>
        /// Get all users
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<User>> GetUsers();
    }
}
