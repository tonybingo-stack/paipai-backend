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
            if(model.Urls?.Count==model.Types?.Count && model.Types?.Count==model.PreviewHs?.Count && model.PreviewHs?.Count == model.PreviewWs?.Count)
            {
                Post entity = _mapper.Map<Post>(model);
                if (entity.Title != null) entity.Title = entity.Title;
                if (entity.Contents != null) entity.Contents = entity.Contents;
                if (entity.Category != null) entity.Category = entity.Category;
                if (entity.Price != null) entity.Price = entity.Price;

                entity.UserName = UserName;
                entity.isDeleted = false;
                var response = await _homeService.CreatePost(entity, model);

                return Ok(response);
            }
            else
            {
                return BadRequest("Number of Urls, Types, PreviewHs, PreviewWs must same!");
            }

        }
        /// <summary> 
        /// Get Posts of User
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<PostViewModel>), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/posts")]
        public async Task<IActionResult> GetPostsOfCommunity()
        {
            return Ok(await _homeService.GetPosts(UserName));
        }
        /// <summary> 
        /// Get Posts for feed page
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<PostFeedViewModel>), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/posts/feed")]
        public async Task<IActionResult> GetPostsForFeed([Required][FromQuery]int offset)
        {
            return Ok(await _homeService.GetPostsForFeed(offset));
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
            if (model.Urls.Count == model.Types.Count && model.Types.Count == model.PreviewHs.Count && model.PreviewHs.Count == model.PreviewWs.Count)
            {
                if (model.Title != null) model.Title = model.Title;
                if (model.Contents != null) model.Contents = model.Contents;
                if (model.Price != null) model.Price = model.Price;
                if (model.Category != null) model.Category = model.Category;

                return Ok(await _homeService.UpdatePost(model));
            }
            else
            {
                return BadRequest("Number of Urls, Types, PreviewHs, PreviewWs must same!");
            }


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
        /// <summary>
        /// User Like Post
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/post/like")]
        public async Task<IActionResult> LikePost([Required][FromForm] Guid postId)
        {
            var response = await _homeService.LikePost(UserName, postId);
            return Ok(response);
        }
        /// <summary>
        /// User unlike Post
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/post/unlike")]
        public async Task<IActionResult> UnLikePost([Required][FromForm] Guid postId)
        {
            var response = await _homeService.UnLikePost(UserName, postId);
            return Ok(response);
        }
    }
}
