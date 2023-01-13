using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;

namespace SignalRHubs.Controllers
{
    public class CommunityController : ApiBaseController
    {
        private readonly IMapper _mapper;
        private readonly IHomeService _homeService;
        public CommunityController(IHomeService service, IUserService userService, IMapper mapper): base(userService)
        {
            _homeService = service;
            _mapper = mapper;
        }
        /// <summary>
        /// Create New Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/create-new-community")]
        public async Task<IActionResult> CreateCommunity([FromForm] CommunityModel model)
        {
            Community entity = _mapper.Map<Community>(model);
            entity.CommunityOwnerName = UserName;
            if(entity.CommunityDescription!=null) entity.CommunityDescription = entity.CommunityDescription.Replace("'", "''");

            return Ok(await _homeService.CreateCommunity(entity));
        }
        /// <summary>
        /// Get all communities
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<CommunityViewModel>), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/community")]
        public async Task<IActionResult> GetAllCommunity()
        {
            return Ok(await _homeService.GetCommunity(UserName));
        }
        /// <summary>
        /// Update Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPut("/community")]
        public async Task<IActionResult> UpdateCommunity([FromForm] CommunityUpdateModel model)
        {
            if (model.Id == null) return BadRequest("Community ID is required!");
            if (model.CommunityName == null && model.CommunityDescription == null && model.CommunityType == null && model.ForegroundImage == null && model.BackgroundImage == null) return BadRequest("At least one field is required!");
            
            Community entity = _mapper.Map<Community>(model);
            if (entity.CommunityDescription != null) entity.CommunityDescription = entity.CommunityDescription.Replace("'", "''");
            entity.CommunityOwnerName = UserName;
            entity.UpdatedAt = DateTime.Now;
            return Ok(await _homeService.UpdateCommunity(entity));
        }
        /// <summary>
        /// Delete Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpDelete("/community")]
        public async Task<IActionResult> DeleteCommunity(Guid id)
        {
            return Ok(await _homeService.DeleteCommunity(id));
        }
        /// <summary> 
        /// Create New Channels
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid),200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/create-new-channel")]
        public async Task<IActionResult> CreateChannel([FromForm] ChannelModel model)
        {
            Channel entity = _mapper.Map<Channel>(model);
            if (entity.ChannelDescription != null) entity.ChannelDescription = entity.ChannelDescription.Replace("'", "''");
            return Ok(await _homeService.CreateChannel(entity));
        }
        /// <summary> 
        /// Get channels of Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(ChannelViewModel),200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/channels")]
        public async Task<IActionResult> GetAllChannels([FromForm] Guid communityID)
        {
            return Ok(await _homeService.GetAllChannels(communityID));
        }
        /// <summary> 
        /// Update channel
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPut("/channel")]
        public async Task<IActionResult> UpdateChannel([FromForm] ChannelUpdateModel model)
        {
            if (model.ChannelId == null) return BadRequest("ChannelID is required!");
            if (model.ChannelName == null && model.ChannelDescription == null) return BadRequest("Name or Description is required!");

            if (model.ChannelDescription != null) model.ChannelDescription = model.ChannelDescription.Replace("'", "''");
            return Ok(await _homeService.UpdateChannel(model));
        }
        /// <summary>
        /// Delete Channel
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpDelete("/channel")]
        public async Task<IActionResult> DeleteChannel(Guid channelId)
        {
            return Ok(await _homeService.DeleteChannel(channelId));
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

            return Ok(await _homeService.CreatePost(entity));
        }
        /// <summary> 
        /// Get Posts of Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<PostViewModel>), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/posts/{communityID}")]
        public async Task<IActionResult> GetPostsOfCommunity([FromRoute] Guid communityID)
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
            if (model.Id == null) return BadRequest("Post ID is required!");
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
        public async Task<IActionResult> DeletePost(Guid postId)
        {
            return Ok(await _homeService.DeletePost(postId));
        }
    }
}
