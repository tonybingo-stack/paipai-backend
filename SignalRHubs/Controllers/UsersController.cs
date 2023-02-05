using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;

namespace SignalRHubs.Controllers
{
    public class UsersController : ApiBaseController
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IConfiguration iconfiguration;
        public UsersController(IUserService userService) : base(userService)
        {
            _userService = userService;
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
            var userSummary = _mapper.Map<UserViewModel>(user);
            return Ok(userSummary);
        }
        /// <summary>
        /// Edit User Profile
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/edit")]
        public async Task<IActionResult> EditUserProfile(EditUserModel model)
        {
            var response = await _userService.EditUserProfile(model);
            return Ok(response);
        }
    }
}
