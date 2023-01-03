using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace SignalRHubs.Controllers
{
    public class UserController : Controller
    {
        private static readonly SecurityKey SigningKey = new SymmetricSecurityKey(Guid.NewGuid().ToByteArray());

        private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();

        public static readonly SigningCredentials SigningCreds = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
        public const string Issuer = "ChatJwt";

        public const string Audience = "ChatJwt";

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

        [HttpGet("login")]
        public IActionResult Login([FromQuery] string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Username and role is required.");
            }

            if (!IsExistingUser(username))
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Role, "admin")
            };

            var claimsIdentity = new ClaimsIdentity(claims);

            var token = JwtTokenHandler.CreateJwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                subject: claimsIdentity,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: SigningCreds
            );

            return Ok(JwtTokenHandler.WriteToken(token));
        }

        private bool IsExistingUser(string username)
        {
            return true;
            //return username.StartsWith("jwt");
        }
    }
}
