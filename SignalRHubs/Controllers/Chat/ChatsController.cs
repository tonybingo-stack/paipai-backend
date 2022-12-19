using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SignalRHubs.Entities;
using SignalRHubs.Hubs;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;

namespace SignalRHubs.Controllers.Chat
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ChatsController : ApiBaseController
    {
        private readonly IChatService _service;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="hubContext"></param>
        /// <param name="mapper"></param>
        /// <param name="userService"></param>
        public ChatsController(IChatService service, IHubContext<ChatHub> hubContext
            , IMapper mapper
            , IUserService userService
            )
        {
            _service = service;
            _hubContext = hubContext;
            _mapper = mapper;
            _userService = userService;

            }

        /// <summary>
        /// Get all chat rooms for user
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<ChatRoomViewModel>), 200)]
        [ProducesResponseType(500)]
        [HttpGet("/users/{userId}/rooms")]
        public async Task<IActionResult> GetChatRooms([FromRoute] string userId)
        {
            return Ok(await _service.GetChatRoomsByUserId(Guid.Parse(userId)));            
        }

        /// <summary>
        /// Get messages of a room by Id
        /// </summary>
        /// <param name="roomId">Chat Room Id</param>
        /// <returns>List of chat messages.</returns>
        [ProducesResponseType(typeof(List<MessageViewModel>), 200)]
        [ProducesResponseType(500)]
        [HttpGet("/rooms/{roomId}")]
        public async Task<IActionResult> GetMessagesByRoomId(string roomId)
        {
            return Ok(await _service.GetMessageByRoomId(Guid.Parse(roomId)));
        }

        /// <summary>
        /// Get details of a message by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Return message details</returns>
        [ProducesResponseType(typeof(Message), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpGet("/messages/{id}")]
        public async Task<IActionResult> GetMessageById(string id)
        {
            return Ok(await _service.GetMessage(Guid.Parse(id)));
        }

        /// <summary>
        /// Save new message (with room generation)
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Message), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("/messages")]
        public async Task<IActionResult> CreateMessage([FromForm] MessageBindingModel model)
        {
            // Get Room Id

            if (model.RoomId == null)
            {
                var (roomId, userNames) = await _service.GetChatRoomByUserIds(new List<string> { UserId.ToString(), model.ReceiverId.ToString() });
                
                if(roomId == null)
                {
                    roomId = Guid.NewGuid();
                    ChatRoom room = new()
                    {
                        Id = roomId.Value,
                        Name = string.Join("_", userNames),
                        IsGroupChat = false
                    };

                    room.Details = new List<ChatRoomDetail>
                    {
                        new ChatRoomDetail
                        {
                            UserId = UserId,
                            RoomId = roomId.Value,
                        },
                        new ChatRoomDetail
                        {
                            UserId = model.ReceiverId,
                            RoomId = roomId.Value,
                        }
                    };

                    await _service.SaveRoom(room);
                }
                model.RoomId = roomId.Value;
            }


            Message message = _mapper.Map<Message>(model);
            message.Id = Guid.NewGuid();
            message.RoomId = model.RoomId.Value;
            message.SenderId = UserId;
            await _service.SaveMessage(message);

            // For now 1x1 chat
            MessageViewModel messageVm = _mapper.Map<MessageViewModel>(message);
            await _hubContext.Clients.User(model.ReceiverId.ToString()).SendAsync("notifyMessage", messageVm);
            // await _hubContext.Clients.All.SendAsync("notifyMessage", messageVm);

            return StatusCode(StatusCodes.Status201Created);
        }

        /// <summary>
        /// Update a message by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(Message), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPut("/messages/{id}")]
        public async Task<IActionResult> Put(string id, [FromForm] MessageBindingModel model)
        {
            Guid messageId = Guid.Parse(id);
            if (model.RoomId == null) return BadRequest("RoomId is required.");

            Message message = await _service.GetMessage(messageId);
            if (message == null) return BadRequest("Message does not exists.");

            message.Content = model.Content;
            message.UpdatedAt = DateTime.Now;
            message.EntityState = DbHelper.Enums.EntityState.Modified;
            await _service.SaveMessage(message);

            // For now 1x1 chat
            MessageViewModel messageVm = _mapper.Map<MessageViewModel>(message);
            message.EntityState = DbHelper.Enums.EntityState.Modified;
            await _hubContext.Clients.All.SendAsync("notifyMessage", messageVm);

            return Ok();
        }

        /// <summary>
        /// Delete a message by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(Message), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpDelete("/messages/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            Message message = await _service.GetMessage(Guid.Parse(id));
            if (message == null) return BadRequest("Message does not exists.");

            message.EntityState = DbHelper.Enums.EntityState.Deleted;
            await _service.SaveMessage(message);

            MessageViewModel messageVm = _mapper.Map<MessageViewModel>(message);
            await _hubContext.Clients.User(UserId.ToString()).SendAsync("notifyDeleteMessage", messageVm);

            return Ok();
        }


        /// <summary>
        /// Get all available users for chat
        /// </summary>
        /// <returns></returns>
        [HttpGet("/users")]
        [ProducesResponseType(typeof(IEnumerable<ReadUserSummaryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ReadUserSummaryModel>>> GetCustomersSummary()
        {
            var users = await _userService.GetUsers();
            var usersSummary = _mapper.Map<IEnumerable<ReadUserSummaryModel>>(users);
            return Ok(usersSummary);
        }
    }
}
