using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Extensions;
using SignalRHubs.Interfaces.Services;
using System.Security.Claims;

namespace SignalRHubs.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public abstract class ApiBaseController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        protected string UserName => User.Identity.GetUserName();

        /// <summary>
        /// 
        /// </summary>
        protected string UserEmail => User.Identity.GetUserEmail();

    }
}
