using DbHelper.Interfaces.Services;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;

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
                $"WHERE dbo.Users.UserName = @Name";
            var response = await _userService.GetDataAsync(query, new {Name = name});
            if (response.Count == 0) return Guid.Empty;
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
                $"NEWID(), " +
                $"@eFirstName, " +
                $"@eLastName, " +
                $"@eUsername, " +
                $"@eNickname, " +
                $"@eEmail, " +
                $"@ePass, " +
                $"@ePhone, " +
                $"@eGender, " +
                $"CURRENT_TIMESTAMP, " +
                $"@eAvatar, " +
                $"@eBack, " +
                $"@eBio" +
                $")";
            var obj = new
            {
                eId = entity.Id,
                eFirstName = entity.FirstName,
                eLastName = entity.LastName,
                eUsername = entity.UserName,
                eNickname = entity.NickName,
                eEmail = entity.Email,
                ePass = entity.Password,
                ePhone = entity.Phone,
                eGender = entity.Gender,
                eAvatar = entity.Avatar,
                eBack = entity.Background,
                eBio = entity.UserBio
            };
            await _userService.GetDataAsync(query, obj);

            return "success";
        }
        /// <summary>
        /// Login User
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<string> LoginUser(UserSigninModel model)
        {

            var query = $"SELECT * FROM Users Where username=@name AND Password = @pass";
            var response = await _userService.GetDataAsync(query, new {name=model.UserName, pass=model.Password});

            if (response.Count == 0) return "User Not Found";
            else return "success";
        }
        /// <summary>
        /// Get User by UserName
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<User> GetUserByUserName(string name)
        {
            var query = $"SELECT * FROM Users WHERE UserName=@username";
            var res = await _userService.GetDataAsync<User>(query, new {username=name});
            if (res.Count == 0) return null;
            return res[0];
        }
        public async Task<List<User>> GetUsers()
        {
            var query = $"SELECT * FROM Users";
            
            var response = await _userService.GetDataAsync(query);

            return response.ToList();
        }

        public async Task<string> UpdateUserAvatar(string url, string username)
        {
            var query = $"UPDATE [dbo].[Users] SET Avatar = @Url WHERE UserName = @Username";
            await _userService.GetDataAsync(query, new { Url=url, Username=username });
            return "Successfully updated user avatar!";
        }
        public async Task<string> UpdateUserBackground(string url, string userName)
        {
            var query = $"UPDATE [dbo].[Users] SET Background = @Url WHERE UserName = @Username";
            await _userService.GetDataAsync(query, new { Url = url, Username = userName });
            return "Successfully updated user background!";
        }
        public async Task<bool> IsValidUserName(string userName)
        {
            var query = $"SELECT * FROM dbo.Users WHERE UserName = @username";
            var result = await _userService.GetDataAsync(query, new {username=userName});

            if (result.Count == 0) return true;
            else return false;
        }

        public async Task<string> AddUserToFriendList(string userName, string username)
        {
            var query = $"INSERT INTO [dbo].[FriendList] VALUES(NEWID(), @user1, @user2, 0);";
            await _userService.GetDataAsync(query, new { user1=userName, user2=username });
            return "Successfully added friend list.";
        }

        public async Task<List<UserViewModel>> GetAllFriends(string userName)
        {
            var query = $"SELECT * FROM dbo.FriendList " +
                $"INNER JOIN dbo.Users ON dbo.FriendList.UserTwo =dbo.Users.UserName " +
                $"WHERE dbo.FriendList.UserOne=@user AND dbo.FriendList.isBlocked=0";
            var response=await _userService.GetDataAsync<UserViewModel>(query, new { user=userName });
            return response.ToList();
        }

        public async Task<string> RemoveUserFromFriend(string userName, string username)
        {
            var query = $"DELETE FROM dbo.FriendList " +
                $"WHERE (UserOne=@user1 AND UserTwo=@user2) OR (UserOne=@user2 AND UserTwo=@user1);";
            await _userService.GetDataAsync(query, new { user1=userName, user2=username });
            return $"{username} removed from friend list.";
        }

        public async Task<string> BlockUser(string userName, string username)
        {
            var query = $"UPDATE dbo.FriendList SET isBlocked=1 " +
                $"WHERE UserOne=@user2 AND UserTwo=@user1;";
            await _userService.GetDataAsync(query, new { user1=userName, user2=username });
            return $"{username} blocked by {userName}!";
        }

        public async Task<string> EditUserProfile(string username, EditUserModel model)
        {
            var query = $"UPDATE dbo.Users " +
                $"SET ";
            bool flag = false;
            if(model.NickName!=null)
            {
                query += "NickName=@uNick ";
                flag = true;
            }
            if(model.Phone!=null)
            {
                if (flag) query += ", ";
                query += "Phone=@uPhone ";
                flag = true;
            }
            if(model.Email!=null)
            {
                if (flag) query += ", ";
                query += " Email=@uEmail ";
                flag = true;
            }
            if (model.Gender!=null)
            {
                if (flag) query += ", ";
                query += " Gender=@uGender ";
                flag = true;
            }
            if (model.Avatar!=null)
            {
                if (flag) query += ", ";
                query += " Avatar=@uAvatar ";
                flag = true;
            }
            if (model.Background!=null)
            {
                if (flag) query += ", ";
                query += " Background=@uBack ";
                flag = true;
            }
            if (model.UserBio!=null)
            {
                if (flag) query += ", ";
                query += " UserBio=@uBio ";
            }

            query += "WHERE UserName = @uName;";
            var param = new
            {
                uName = username,
                uNick = model.NickName,
                uPhone = model.Phone,
                uEmail = model.Email,
                uGender = model.Gender,
                uAvatar = model.Avatar,
                uBack = model.Background,
                uBio = model.UserBio
            };
            Console.WriteLine("user name:" + username+":"+param.ToString()+":"+query);
            await _userService.GetDataAsync(query, param);
            return "Updated User Profile.";
        }
    }
}
