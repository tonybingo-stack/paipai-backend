using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SignalRHubs.Controllers
{
    public class HomeController : Controller
    {
        private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();

        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IConfiguration iconfiguration;
        public HomeController(IUserService service, IMapper mapper, IConfiguration iconfiguration)
        {
            this._userService = service;
            _mapper = mapper;
            this.iconfiguration = iconfiguration;
        }
        private string TokenGenerator(string name)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, name),
                new Claim(ClaimTypes.Role, "admin")
            };
            SecurityKey SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(iconfiguration["Jwt:Key"]));
            SigningCredentials SigningCreds = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);

            var claimsIdentity = new ClaimsIdentity(claims);

            var token = JwtTokenHandler.CreateJwtSecurityToken(
                issuer: iconfiguration["Jwt:Issuer"],
                audience: iconfiguration["Jwt:Audience"],
                subject: claimsIdentity,
                expires: DateTime.UtcNow.AddMinutes(65),
                signingCredentials: SigningCreds
            );
            var userToken = JwtTokenHandler.WriteToken(token);
            return userToken;
        }
        /// <summary>
        /// Create New User
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/signup")]
        public async Task<IActionResult> CreateUser(CreateUserModel model)
        {
            User user = _mapper.Map<User>(model);
            if (await _userService.IsValidUserName(model.UserName))
            {
                return Ok(await _userService.CreateUser(user));
            }
            return BadRequest("Invalid user name!");
        }

        /// <summary>
        /// User Login
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserSignupModel) ,200)]
        [ProducesResponseType(400)]
        [HttpPost("/signin")]
        public async Task<IActionResult> Login([FromForm] UserSigninModel user)
        {
            if (string.IsNullOrEmpty(user.UserName))
            {
                return BadRequest("Username is required.");
            }

            if (!await IsExistingUser(user.UserName, user.Password))
            {
                return Unauthorized();
            }
            // Generate Token
            var userToken = TokenGenerator(user.UserName);

            var response = await _userService.GetUserByUserName(user.UserName);

            UserSignupModel res = _mapper.Map<UserSignupModel>(response);
            res.Token = userToken;

            //return Ok(userToken);
            return Ok(res);
        }

        private async Task<bool> IsExistingUser(string username, string password)
        {
            UserSigninModel user = new UserSigninModel();
            user.UserName = username;
            user.Password = password;
            var res = await _userService.LoginUser(user);

            return res == "success";
        }
        /// <summary>
        /// UserName Validation Check
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [HttpGet("/check/username/{username}")]
        public async Task<IActionResult> UserNameValidationCheck([FromRoute] string username)
        {
            var result = await _userService.IsValidUserName(username);

            return Ok(result);

        }
    }
}
