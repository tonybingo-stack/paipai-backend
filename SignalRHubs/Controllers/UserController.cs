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
        private readonly IUserService _userService;
        public UserController(IUserService service)
        {
            this._userService = service;
        }
        /// <summary>
        /// Create New User
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/createuser")]
        public async Task<IActionResult> CreateUser(UserCredential user)
        {
            return Ok(await _userService.CreateUser(user));
            //return Ok(myuser);
        }

        /// <summary>
        /// User Login
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginCredential user)
        {
            return Ok(await _userService.LoginUser(user));
        }
    }
}
