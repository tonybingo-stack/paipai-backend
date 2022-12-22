using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;

namespace SignalRHubs.Controllers
{

    public class UserController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        public UserController(IUserService service, IMapper mapper)
        {
            this._userService = service;
            _mapper = mapper;
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
            //return Ok(myuser);
        }

        /// <summary>
        /// User Login
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/signin")]
        public async Task<IActionResult> Login(UserModel model)
        {
            LoginCredential user = _mapper.Map<LoginCredential>(model);
            return Ok(await _userService.LoginUser(user));
        }
    }
}
