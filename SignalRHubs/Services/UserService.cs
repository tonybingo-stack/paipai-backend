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
                $"WHERE dbo.Users.UserName = N'{name}'";
            var response = await _userService.GetDataAsync(query);

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
                $"N'{ entity.FirstName }', " +
                $"N'{ entity.LastName }', " +
                $"N'{ entity.UserName }', " +
                $"N'{ entity.NickName }', " +
                $"N'{ entity.Email }', " +
                $"N'{ entity.Password }', " +
                $"N'{ entity.Phone }', " +
                $"{ entity.Gender }, " +
                $"CURRENT_TIMESTAMP, " +
                $"N'{entity.Avatar}'" +
                $")";
            await _userService.GetDataAsync(query);

            return "success";
        }
        /// <summary>
        /// Login User
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<string> LoginUser(UserSigninModel model)
        {

            var query = $"SELECT * FROM Users Where username=N'{ model.UserName }'";
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
            var query = $"SELECT * FROM Users WHERE UserName=N'{name}'";
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
            var query = $"UPDATE [dbo].[Users] SET Avatar = N'{url}' WHERE UserName = N'{username}'";
            await _userService.GetDataAsync(query);
            return "Successfully updated user avatar!";
        }
        public async Task<string> UpdateUserBackground(string url, string userName)
        {
            var query = $"UPDATE [dbo].[Users] SET Background = N'{url}' WHERE UserName = N'{userName}'";
            await _userService.GetDataAsync(query);
            return "Successfully updated user background!";
        }
        public async Task<bool> IsValidUserName(string userName)
        {
            var query = $"SELECT * FROM dbo.Users WHERE UserName = N'{userName}'";
            var result = await _userService.GetDataAsync(query);

            if (result.Count == 0) return true;
            else return false;
        }

        public async Task<string> AddUserToFriendList(string userName, string username)
        {
            var query = $"INSERT INTO [dbo].[FriendList] VALUES(NEWID(), '{userName}', '{username}', 0);" +
                $"INSERT INTO [dbo].[FriendList] VALUES(NEWID(), '{username}', '{userName}', 0);";
            await _userService.GetDataAsync(query);
            return "Successfully added friend list.";
        }

        public async Task<List<UserViewModel>> GetAllFriends(string userName)
        {
            var query = $"SELECT * FROM dbo.FriendList " +
                $"INNER JOIN dbo.Users ON dbo.FriendList.UserTwo =dbo.Users.UserName " +
                $"WHERE dbo.FriendList.UserOne='{userName}' AND dbo.FriendList.isBlocked=0";
            var response=await _userService.GetDataAsync<UserViewModel>(query);
            return response.ToList();
        }

        public async Task<string> RemoveUserFromFriend(string userName, string username)
        {
            var query = $"DELETE FROM dbo.FriendList " +
                $"WHERE (UserOne='{userName}' AND UserTwo='{username}') OR (UserOne='{username}' AND UserTwo='{userName}');";
            await _userService.GetDataAsync(query);
            return $"{username} removed from friend list.";
        }

        public async Task<string> BlockUser(string userName, string username)
        {
            var query = $"UPDATE dbo.FriendList SET isBlocked=1 " +
                $"WHERE (UserOne='{userName}' AND UserTwo='{username}') OR (UserOne='{username}' AND UserTwo='{userName}');";
            await _userService.GetDataAsync(query);
            return $"{username} blocked!";
        }
    }
}
