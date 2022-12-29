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
        Task<Guid> GetIdByUserName(string name);
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
        Task<User> GetUserByID(Guid userId);

        /// <summary>
        /// Get all users
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        Task<List<User>> GetUsers();

        /// <summary>
        /// Update Hub connectionID of the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="connectionID"></param>
        /// <returns></returns>
        Task UpdateHubConnectionID(Guid UserID, string connectionID);
    }
}
