// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SignalRHubs.Models;

namespace SignalRHubs
{
    public class UserController : Controller
    {
        private static readonly SecurityKey SigningKey = new SymmetricSecurityKey(Guid.NewGuid().ToByteArray());

        private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();

        public static readonly SigningCredentials SigningCreds = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);

        private readonly IConfiguration iconfiguration;
        public UserController(IConfiguration iconfiguration)
        {
            this.iconfiguration = iconfiguration;
        }

        [HttpGet("users")]
        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = new List<User>();
            using (var connection = new SqlConnection(iconfiguration.GetConnectionString("DbConnection")))
            {
                var query = "SELECT * FROM Users";
                connection.Open();
                using SqlCommand command = new SqlCommand(query, connection);
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var user = new User()
                    {
                        Id = Guid.Parse(reader["Id"].ToString()),
                        Username = reader["username"].ToString(),
                        Password = reader["password"].ToString()
                    };
                    users.Add(user);
                }
            }
            return users;
        }

        [HttpGet("login")]
        public IActionResult Login([FromQuery] string username, [FromQuery] string role)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
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
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims);

            var token = JwtTokenHandler.CreateJwtSecurityToken(
                issuer: iconfiguration.GetConnectionString("Jwt:Issuer"),
                audience: iconfiguration.GetConnectionString("Jwt:Audience"),
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
