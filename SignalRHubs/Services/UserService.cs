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
        private readonly IDapperService<UserCredential> _userService01;
        private readonly SqlConnection _connection;
        private readonly IConfiguration iconfiguration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repository"></param>
        public UserService(IDapperService<User> dapperService, IDapperService<UserCredential> testService, IConfiguration iconfiguration)
        {
            _userService = dapperService;
            _userService01 = testService;
            _connection = dapperService.Connection;
            this.iconfiguration = iconfiguration;
        }
        public async Task<Guid> GetIdByUserName(string name)
        {
            var query = $"SELECT dbo.Users.ID " +
                $"FROM dbo.Users " +
                $"WHERE dbo.Users.UserName = @UserName";
            var response = await _userService.GetDataAsync(query, new { UserName = name });

            return response[0].Id;
        }
        public string Authenticate(UserModel entity)
        {
            var issuer = iconfiguration["Jwt:Issuer"];
            var audience = iconfiguration["Jwt:Audience"];
            var key = Encoding.ASCII.GetBytes(iconfiguration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, entity.UserName),
                new Claim(JwtRegisteredClaimNames.Email, entity.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
             }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials
                (new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            var stringToken = tokenHandler.WriteToken(token);

            return stringToken;
        }
    

        /// <summary>
        /// Add a new customer
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<string> CreateUser(UserCredential entity)
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
                $"null" +
                $")";
            await _userService.GetDataAsync(query);

            query = $"Insert into HubUsers Values(" +
                $"'NEWID()', " +
                $"'{ entity.Id }', " +
                $"null" +
                $")";
            await _userService.GetDataAsync(query);

            return "success";
        }
        /// <summary>
        /// Login User
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<string> LoginUser(LoginCredential entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var query = $"SELECT * FROM Users Where username='{ entity.UserName }'";
            var response = await _userService01.GetDataAsync(query);

            if (response.Count == 0) return "User Not Found";

            if (response.FirstOrDefault().Password == entity.Password)
            {
                return Authenticate(new UserModel { UserName = entity.UserName,Password = entity.Password});
            }
            else return "Incorrect Password";
        }
        /// <summary>
        /// Get User by ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<User> GetUserByID(Guid userId)
        {
            var query = $"SELECT * FROM Users WHERE Id='{userId}'";
            return await _userService.GetFirstOrDefaultAsync<User>(query);
        }

        public async Task<List<User>> GetUsers()
        {
            var query = $"SELECT * FROM Users";
            
            var response = await _userService.GetDataAsync(query);

            return response.ToList();
        }
        public async Task UpdateHubConnectionID(Guid UserID, string connectionID)
        {
            var query = $"UPDATE dbo.HubUsers " +
                $"SET ConnectionID = { connectionID } " +
                $"WHERE dbo.HubUsers.UserID = { UserID }";

            await _userService.GetDataAsync(query);
        }


    }
}
