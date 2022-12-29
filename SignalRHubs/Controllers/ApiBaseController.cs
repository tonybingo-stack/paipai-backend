using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Extensions;
using SignalRHubs.Interfaces.Services;

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
        protected IUserService UserService;
        public ApiBaseController(IUserService userService)
        {
            UserService = userService;
        }
        /// <summary>
        /// 
        /// </summary> 
        protected string UserName
        {
            get
            {
                return User.Identity.GetUserName();
            }
        }

        /// <summary>
        /// Get User ID
        /// </summary>
        protected Task<Guid> UserId
        {
            get
            {
                return UserService.GetIdByUserName(User.Identity.GetUserName());
            }
        }
    }
}
