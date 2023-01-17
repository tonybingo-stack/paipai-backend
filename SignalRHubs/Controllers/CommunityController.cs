using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Lib;
using SignalRHubs.Models;
using System.ComponentModel.DataAnnotations;

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
            entity.CommunityName = entity.CommunityName.Replace("'", "''");
            entity.CommunityOwnerName = UserName;
            if(entity.CommunityDescription!=null) entity.CommunityDescription = entity.CommunityDescription.Replace("'", "''");

            var response = await _homeService.CreateCommunity(entity);

            GlobalModule.NumberOfUsers[response.ToString()] = 1;
            GlobalModule.NumberOfPosts[response.ToString()] = 0;
            GlobalModule.NumberOfActiveUser[response.ToString()] = 1;

            return Ok(response);
        }
        ///// <summary>
        ///// Get all communities user created
        ///// </summary>
        ///// <returns></returns>
        //[ProducesResponseType(typeof(List<CommunityViewModel>), 200)]
        //[ProducesResponseType(typeof(NotFoundResult), 400)]
        //[HttpGet("/community")]
        //public async Task<IActionResult> GetAllCommunity()
        //{
        //    return Ok(await _homeService.GetCommunity(UserName));
        //}
        /// <summary>
        /// Update Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPut("/community")]
        public async Task<IActionResult> UpdateCommunity([FromForm] CommunityUpdateModel model)
        {
            // check user role
            CommunityMember m = await _homeService.GetUserRole(UserName, model.Id);
            if (m.UserRole > 0) return BadRequest("UserRole is not enough to perform this action!");

            if (model.Id == null) return BadRequest("Community ID is required!");
            if (model.CommunityName == null && model.CommunityDescription == null && model.CommunityType == null && model.ForegroundImage == null && model.BackgroundImage == null) return BadRequest("At least one field is required!");
            
            Community entity = _mapper.Map<Community>(model);
            if (entity.CommunityName != null) entity.CommunityName = entity.CommunityName.Replace("'", "''");
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
        public async Task<IActionResult> DeleteCommunity([Required] Guid id)
        {
            // check user role
            CommunityMember m = await _homeService.GetUserRole(UserName, id);
            if (m.UserRole > 0) return BadRequest("UserRole is not enough to perform this action!");

            // Update referenced table
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
            // check user role
            CommunityMember m = await _homeService.GetUserRole(UserName, model.ChannelCommunityId);
            if (m.UserRole > 1) return BadRequest("User Role is not enough to perform this action!");

            Channel entity = _mapper.Map<Channel>(model);
            entity.ChannelOwnerName = UserName;
            entity.ChannelName = entity.ChannelName.Replace("'", "''");
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
        public async Task<IActionResult> GetAllChannels([FromForm][Required] Guid communityID)
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
            // Check User Role
            // Get current owner name from db, compare it with UserName.
            Channel c = await _homeService.GetChannelById(model.ChannelId);
            if (c.ChannelOwnerName != UserName) return BadRequest("You are not owner of this channel!");
            if (model.ChannelName == null && model.ChannelDescription == null) return BadRequest("Name or Description is required!");

            if (model.ChannelDescription != null) model.ChannelDescription = model.ChannelDescription.Replace("'", "''");
            if (model.ChannelName != null) model.ChannelName = model.ChannelName.Replace("'", "''");

            return Ok(await _homeService.UpdateChannel(model));
        }
        /// <summary>
        /// Delete Channel
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpDelete("/channel")]
        public async Task<IActionResult> DeleteChannel([Required] Guid channelId)
        {
            // Check User Role
            Community com = await _homeService.GetCommunityFromChannelId(channelId);
            if (com.CommunityOwnerName != UserName) return BadRequest("Only owner of community can delete this channel!");

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
        /// <summary>
        /// Get all communities user joined
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<CommunityViewModel>), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/joined-community")]
        public async Task<IActionResult> GetAllJoinedCommunity()
        {
            return Ok(await _homeService.GetJoinedCommunity(UserName));
        }
        /// <summary>
        /// Join a community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/community/join")]
        public async Task<IActionResult> JoinCommunity([FromForm][Required] Guid communityId)
        {
            GlobalModule.NumberOfUsers[communityId.ToString()] = (int)GlobalModule.NumberOfUsers[communityId.ToString()] + 1;
            return Ok(await _homeService.JoinCommunity(UserName, communityId));
        }
        /// <summary>
        /// Exit from community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/community/exit")]
        public async Task<IActionResult> ExitCommunity([FromForm][Required] Guid communityId)
        {
            GlobalModule.NumberOfUsers[communityId.ToString()] = (int)GlobalModule.NumberOfUsers[communityId.ToString()] - 1;
            return Ok(await _homeService.ExitCommunity(UserName, communityId));
        }

        /// <summary>
        /// Create Event
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/create-new-event")]
        public async Task<IActionResult> CreateEvent([FromForm] EventCreateModel model)
        {
            // check user role
            Channel c=await _homeService.GetChannelById(model.ChannelId);
            CommunityMember m = await _homeService.GetUserRole(UserName, c.ChannelCommunityId);
            if (m.UserRole > 1) return BadRequest("User role is not enough to perform this action!");

            Event e=_mapper.Map<Event>(model);
            e.Title = e.Title.Replace("'", "''");
            if (e.Description != null) e.Description = e.Description.Replace("'", "''");
            if (e.Image != null) e.Image = e.Image.Replace("'", "''");
            e.HostName = UserName;
            
            return Ok(await _homeService.CreateEvent(e));
        }
        /// <summary>
        /// Get Events of Channel
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(EventViewModel), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/events")]
        public async Task<IActionResult> GetAllEvent([Required] Guid ChannelId)
        {
            return Ok(await _homeService.GetAllEvent(ChannelId));
        }
        /// <summary>
        /// Update Event
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPut("/event")]
        public async Task<IActionResult> UpdateEvent([FromForm] EventUpdateModel model)
        {
            // check user role
            Event e = await _homeService.GetEventByID(model.Id);
            if (e == null) return BadRequest("Event not exist!");
            if (e.HostName != UserName) return BadRequest("Only host can update event!");

            if (model.Title != null) e.Title = model.Title.Replace("'", "''");
            if (model.Description != null) e.Description = model.Description.Replace("'", "''");
            if (model.Access != null) e.Access = model.Access.Replace("'", "''");
            if (model.Image != null) e.Image = model.Image.Replace("'", "''");
            if (model.Title != null) e.Time = model.Time;
            if(model.Limit != null) e.Limit = model.Limit.Value;

            return Ok(await _homeService.UpdateEvent(e));
        }
        /// <summary>
        /// Delete Event
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpDelete("/event")]
        public async Task<IActionResult> DeleteEvent([FromForm][Required] Guid Id)
        {
            // check user role
            Event e = await _homeService.GetEventByID(Id);
            if (e == null) return BadRequest("Event not exist!");
            // Check User Role
            Community com = await _homeService.GetCommunityFromChannelId(e.ChannelId);
            if (com.CommunityOwnerName != UserName) return BadRequest("Only owner of community can delete this channel!");

            return Ok(await _homeService.DeleteEvent(Id));
        }
    }
}
