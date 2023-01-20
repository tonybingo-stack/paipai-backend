using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using SignalRHubs.Entities;
using SignalRHubs.Hubs;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Lib;
using SignalRHubs.Models;
using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Controllers
{
    public class PostController : ApiBaseController
    {
        private readonly IMapper _mapper;
        private readonly IHomeService _homeService;
        private readonly IHubContext<ChatHub> _hubContext;
        public PostController(IHomeService service, IUserService userService, IMapper mapper, IHubContext<ChatHub> hubContext) : base(userService)
        {
            _homeService = service;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        /// <summary> 
        /// Create New Post
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/create-new-post")]
        public async Task<IActionResult> CreatePost([FromForm] PostCreateModel model)
        {
            Post entity = _mapper.Map<Post>(model);
            if (entity.Title != null) entity.Title = entity.Title.Replace("'", "''");
            if (entity.Contents != null) entity.Contents = entity.Contents.Replace("'", "''");
            if (entity.Category != null) entity.Category = entity.Category.Replace("'", "''");
            if (entity.Price != null) entity.Price = entity.Price.Replace("'", "''");

            entity.UserName = UserName;
            entity.isDeleted = false;
            var response = await _homeService.CreatePost(entity);
            GlobalModule.NumberOfPosts[model.CommunityID.ToString()] = (bool)GlobalModule.NumberOfPosts[model.CommunityID.ToString()] ? 1 : (int)GlobalModule.NumberOfPosts[model.CommunityID.ToString()] + 1;

            return Ok(response);
        }
        /// <summary> 
        /// Get Posts of Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<PostViewModel>), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/posts")]
        public async Task<IActionResult> GetPostsOfCommunity([FromQuery][Required] Guid communityID)
        {
            return Ok(await _homeService.GetPosts(communityID, UserName));
        }
        /// <summary> 
        /// Update Post
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPut("/post")]
        public async Task<IActionResult> UpdatePost([FromForm] PostUpdateModel model)
        {
            if (model.Title == null && model.Contents == null && model.Price == null && model.Category == null) return BadRequest("At lease one field is required!");

            if (model.Title != null) model.Title = model.Title.Replace("'", "''");
            if (model.Contents != null) model.Contents = model.Contents.Replace("'", "''");
            if (model.Price != null) model.Price = model.Price.Replace("'", "''");
            if (model.Category != null) model.Category = model.Category.Replace("'", "''");

            return Ok(await _homeService.UpdatePost(model));
        }
        /// <summary>
        /// Delete Post
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpDelete("/post")]
        public async Task<IActionResult> DeletePost([Required] Guid postId)
        {
            var response = await _homeService.DeletePost(postId);
            return Ok(response);
        }
    }
}
