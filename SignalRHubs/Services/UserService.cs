using DbHelper.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SignalRHubs.Hubs;
using Microsoft.AspNet.SignalR;

namespace SignalRHubs.Services
{
    public class UserService : IUserService
    {

        private readonly IDapperService<User> _userService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repository"></param>
        public UserService(IDapperService<User> dapperService)
        {
            _userService = dapperService;
        }
        public async Task<Guid> GetIdByUserName(string name)
        {
            var query = $"SELECT dbo.Users.ID " +
                $"FROM dbo.Users " +
                $"WHERE dbo.Users.UserName = @UserName";
            var response = await _userService.GetDataAsync(query, new { UserName = name });

            return response[0].Id;
        }    

        /// <summary>
        /// Add a new customer
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<string> CreateUser(User entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var query = $"Insert into Users Values(" +
                $"'{ entity.Id }', " +
                $"'{ entity.FirstName }', " +
                $"'{ entity.LastName }', " +
                $"'{ entity.UserName }', " +
                $"'{ entity.NickName }', " +
                $"'{ entity.Email }', " +
                $"'{ entity.Password }', " +
                $"'{ entity.Phone }', " +
                $"{ entity.Gender }, " +
                $"CURRENT_TIMESTAMP, " +
                $"'{entity.Avatar}'" +
                $")";
            await _userService.GetDataAsync(query);

            return "success";
        }
        /// <summary>
        /// Login User
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<string> LoginUser(UserModel model)
        {

            var query = $"SELECT * FROM Users Where username='{ model.UserName }'";
            var response = await _userService.GetDataAsync(query);

            if (response.Count == 0) return "User Not Found";

            if (response.FirstOrDefault().Password == model.Password)
            {
                return "success";
            }
            else return "Incorrect Password";
        }
        /// <summary>
        /// Get User by UserName
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<User> GetUserByUserName(string name)
        {
            var query = $"SELECT * FROM Users WHERE UserName='{name}'";
            return await _userService.GetFirstOrDefaultAsync<User>(query);
        }
        public async Task<List<User>> GetUsers()
        {
            var query = $"SELECT * FROM Users";
            
            var response = await _userService.GetDataAsync(query);

            return response.ToList();
        }

        public async Task<string> UpdateUserAvatar(string url, string username)
        {
            var query = $"UPDATE [dbo].[Users] SET Avatar = '{url}' WHERE UserName = '{username}'";
            await _userService.GetDataAsync(query);
            return "Successfully updated user avatar!";
        }
    }
}
