using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Extensions;
using System.Security.Claims;

namespace SignalRHubs.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public abstract class ApiBaseController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        protected Guid UserId => User.Identity.GetUserId();
        /// <summary>
        /// 
        /// </summary>
        protected string UserEmail => User.Identity.GetUserEmail();
    }
}
