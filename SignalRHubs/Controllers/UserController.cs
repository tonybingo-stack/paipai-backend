using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
    public class UserController : Controller
    {
        private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();

        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IConfiguration iconfiguration;
        public UserController(IUserService service, IMapper mapper, IConfiguration iconfiguration)
        {
            this._userService = service;
            _mapper = mapper;
            this.iconfiguration = iconfiguration;
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
            UserCredential user = _mapper.Map<UserCredential>(model);
            return Ok(await _userService.CreateUser(user));
        }

        /// <summary>
        /// User Login
        /// </summary>
        /// <returns></returns>
        //[ProducesResponseType(200)]
        //[ProducesResponseType(400)]
        //[HttpPost("/signin")]
        //public async Task<IActionResult> SignIn(UserModel model)
        //{
        //    LoginCredential user = _mapper.Map<LoginCredential>(model);
        //    return Ok(await _userService.LoginUser(user));
        //}

        /// <summary>
        /// User Login
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpGet("/signin")]
        public async Task<IActionResult> Login([FromQuery] string username, [FromQuery] string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Username is required.");
            }

            if (!await IsExistingUser(username, password))
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Role, "admin")
            };
            SecurityKey SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(iconfiguration["Jwt:Key"]));
            SigningCredentials SigningCreds = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
            
            var claimsIdentity = new ClaimsIdentity(claims);

            var token = JwtTokenHandler.CreateJwtSecurityToken(
                issuer: iconfiguration["Jwt:Issuer"],
                audience: iconfiguration["Jwt:Audience"],
                subject: claimsIdentity,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: SigningCreds
            );

            return Ok(JwtTokenHandler.WriteToken(token));
        }

        private async Task<bool> IsExistingUser(string username, string password)
        {
            LoginCredential user = new LoginCredential();
            user.UserName = username;
            user.Password = password;
            var res = await _userService.LoginUser(user);

            return res == "success";
        }
    }
}
