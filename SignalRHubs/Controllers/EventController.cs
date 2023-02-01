using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRHubs.Entities;
using SignalRHubs.Hubs;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Lib;
using SignalRHubs.Models;
using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Controllers
{
    public class EventController : ApiBaseController
    {
        private readonly IMapper _mapper;
        private readonly IHomeService _homeService;
        private readonly IHubContext<ChatHub> _hubContext;
        public EventController(IHomeService service, IUserService userService, IMapper mapper, IHubContext<ChatHub> hubContext) : base(userService)
        {
            _homeService = service;
            _mapper = mapper;
            _hubContext = hubContext;
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
            CommunityMember m = await _homeService.GetUserRole(UserName, model.CommunityId);
            if (m.UserRole > 1) return BadRequest("User role is not enough to perform this action!");

            Event e = _mapper.Map<Event>(model);
            e.Title = e.Title.Replace("'", "''");
            if (e.Description != null) e.Description = e.Description;
            if (e.Image != null) e.Image = e.Image;
            e.HostName = UserName;

            return Ok(await _homeService.CreateEvent(e));
        }
        /// <summary>
        /// Get Events of Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(EventViewModel), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/events")]
        public async Task<IActionResult> GetAllEvent([Required] Guid CommunityId)
        {
            return Ok(await _homeService.GetAllEvent(CommunityId));
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

            if (model.Title != null) e.Title = model.Title;
            if (model.Description != null) e.Description = model.Description;
            if (model.Access != null) e.Access = model.Access;
            if (model.Image != null) e.Image = model.Image;
            if (model.Title != null) e.Time = model.Time;
            if (model.Limit != null) e.Limit = model.Limit.Value;

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
            Community com = await _homeService.GetCommunityById(e.CommunityId);
            if (com.CommunityOwnerName != UserName) return BadRequest("Only owner of community can delete this channel!");

            return Ok(await _homeService.DeleteEvent(Id));
        }
    }
}
