using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SignalRHubs.Controllers
{
    public class UsersController : ApiBaseController
    {
        private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();

        private readonly IMapper _mapper;
        private readonly IConfiguration iconfiguration;
        private readonly IUserService _userService;
        public UsersController(IUserService userService, IMapper mapper, IConfiguration iconfiguration) : base(userService)
        {
            _userService = userService;
            _mapper = mapper;
            this.iconfiguration = iconfiguration;
        }
        /// <summary>
        /// Refresh Token
        /// </summary>
        /// <returns></returns>
        [HttpGet("/token")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<string> GetToken()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, UserName),
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
        /// Get all available users for chat
        /// </summary>
        /// <returns></returns>
        [HttpGet("/users")]
        [ProducesResponseType(typeof(IEnumerable<UserViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserViewModel>>> GetCustomersSummary()
        {
            var users = await _userService.GetUsers();
            var usersSummary = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return Ok(usersSummary);
        }
        /// <summary>
        /// Get User by UserName
        /// </summary>
        /// <returns></returns>
        [HttpGet("/user/{username}")]
        [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserViewModel>> GetCustomerSummaryById([FromRoute] string username)
        {
            var user = await _userService.GetUserByUserName(username);
            UserViewModel userSummary = _mapper.Map<UserViewModel>(user);
            return Ok(userSummary);
        }
        /// <summary>
        /// Edit User Profile
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/user/edit")]
        public async Task<IActionResult> EditUserProfile([FromForm] EditUserModel model)
        {
            if (model.NickName == null && model.Email == null && model.Avatar == null && model.Phone == null && model.Gender == null && model.Background == null && model.UserBio == null) return BadRequest("At least one field is required");
            var response = await _userService.EditUserProfile(UserName, model);
            return Ok(response);
        }
    }
}
