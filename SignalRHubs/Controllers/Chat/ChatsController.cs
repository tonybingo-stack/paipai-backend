﻿using AutoMapper;
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
            ):base(userService)
        {
            _service = service;
            _hubContext = hubContext;
            _mapper = mapper;
            _userService = userService;

            }

        /// <summary>
        /// Get all chat channels for user
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<ChatChannelViewModel>), 200)]
        [ProducesResponseType(500)]
        [HttpGet("/users/{userId}/channels")]
        public async Task<IActionResult> GetChatChannels([FromRoute] string userId)
        {
            return Ok(await _service.GetChatChannelsByUserId(Guid.Parse(userId)));            
        }

        /// <summary>
        /// Get messages of a channel by Id
        /// </summary>
        /// <param name="channelId">Chat Channel Id</param>
        /// <returns>List of chat messages.</returns>
        [ProducesResponseType(typeof(List<MessageViewModel>), 200)]
        [ProducesResponseType(500)]
        [HttpGet("/message/{channelId}")]
        public async Task<IActionResult> GetMessagesByRoomId(string channelId)
        {
            return Ok(await _service.GetMessageByChannelId(Guid.Parse(channelId)));
        }

        /// <summary>
        /// Get details of a message by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Return message details</returns>
        [ProducesResponseType(typeof(MessageViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpGet("/messages/{id}")]
        public async Task<IActionResult> GetMessageById(string id)
        {
            return Ok(await _service.GetMessage(Guid.Parse(id)));
        }

        /// <summary>
        /// Save new message (with channel generation)
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(MessageViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("/send-message")]
        public async Task<IActionResult> SendMessage([FromForm] MessageBindingModel model)
        {

            // Get Channel Id
            if (model.ChannelId == null)
            {
                //if (model.CommunityId == null) return BadRequest("ChannelID and CommunityID can not be null at the same time!");

                var (channelId, userNames) = await _service.GetChatChannelByUserIds(new List<string> { UserId.ToString(), model.ReceiverId.ToString() });

                if (channelId == null)
                {
                    channelId = Guid.NewGuid();
                    Channel _channel = new()
                    {
                        ChannelId = channelId.Value,
                        ChannelName = String.Join("_", userNames),
                        ChannelDescription = "This is new generated channel by new message",
                        //ChannelCommunityId = model.CommunityId.Value,
                    };
                    //ChatRoom room = new()
                    //{
                    //    Id = channelId.Value,
                    //    Name = string.Join("_", userNames),
                    //    IsGroupChat = false
                    //};

                    //room.Details = new List<ChatRoomDetail>
                    //{
                    //    new ChatRoomDetail
                    //    {
                    //        UserId = UserId,
                    //        RoomId = channelId.Value,
                    //    },
                    //    new ChatRoomDetail
                    //    {
                    //        UserId = model.ReceiverId,
                    //        RoomId = channelId.Value,
                    //    }
                    //};

                    await _service.SaveChannel(_channel);
                }

                model.ChannelId = channelId.Value;
            }

            Message message = _mapper.Map<Message>(model);
            message.Id = Guid.NewGuid();
            message.ChannelId = model.ChannelId.Value;
            message.SenderId = await UserId;

            await _service.SaveMessage(message);
            // Update ChatHistory table by sender and receiver.
            ChatHistoryModel chatHistoryModel = new ChatHistoryModel();
            chatHistoryModel.ID = Guid.NewGuid();
            chatHistoryModel.UserID = message.SenderId;
            chatHistoryModel.MessageID = message.Id;
            await _service.CreateOrUpdateChatHistory(chatHistoryModel);

            chatHistoryModel.ID = Guid.NewGuid();
            chatHistoryModel.UserID = message.ReceiverId;
            chatHistoryModel.MessageID = message.Id;
            await _service.CreateOrUpdateChatHistory(chatHistoryModel);

            // For now 1x1 chat
            MessageViewModel messageVm = _mapper.Map<MessageViewModel>(message);
            await _hubContext.Clients.User(model.ReceiverId.ToString()).SendAsync("notifyMessage", messageVm);
            //await _hubContext.Clients.All.SendAsync("ReceiveMessage", model.Content);
            // await _hubContext.Clients.All.SendAsync("notifyMessage", messageVm);

            //return Ok(UserId);
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
        public async Task<IActionResult> Put(string id, [FromForm] MessageUpdateModel model)
        {
            Guid messageId = Guid.Parse(id);
            //if (model.ChannelId == null) return BadRequest("Channel ID is required.");

            Message message = _mapper.Map<Message>(await _service.GetMessage(messageId));
            if (message == null) return BadRequest("Message does not exists.");

            message.Content = model.Content;
            message.UpdatedAt = DateTime.Now;
            message.EntityState = DbHelper.Enums.EntityState.Modified;
            //await _service.SaveMessage(message);
            await _service.PutMessage(messageId, message);

            // For now 1x1 chat
            MessageViewModel messageVm = _mapper.Map<MessageViewModel>(message);
            //message.EntityState = DbHelper.Enums.EntityState.Modified;
            await _hubContext.Clients.All.SendAsync("notifyMessage", messageVm);

            return Ok(message);
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
            //Get Userid from UserName
            //Guid UserId = await _userService.GetIdByUserName(UserName);

            Message message = _mapper.Map<Message>(await _service.GetMessage(Guid.Parse(id)));
            if (message == null) return BadRequest("Message does not exists.");

            message.EntityState = DbHelper.Enums.EntityState.Deleted;
            //await _service.SaveMessage(message);
            await _service.DeleteMessage(Guid.Parse(id));

            MessageViewModel messageVm = _mapper.Map<MessageViewModel>(message);
            await _hubContext.Clients.User(UserId.ToString()).SendAsync("notifyDeleteMessage", messageVm);

            return Ok(message);
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
        /// <summary>
        /// Get User by ID
        /// </summary>
        /// <returns></returns>
        [HttpGet("/user/{id}")]
        [ProducesResponseType(typeof(ReadUserSummaryModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReadUserSummaryModel>> GetCustomerSummaryById([FromRoute] string id)
        {
            var user = await _userService.GetUserByID(Guid.Parse(id));
            var userSummary = _mapper.Map<ReadUserSummaryModel>(user);
            return Ok(userSummary);
        }
        /// <summary>
        /// Get All Chat cards
        /// </summary>
        /// <returns></returns>
        [HttpGet("/chathistory")]
        [ProducesResponseType(typeof(ChatCardModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ChatCardModel>>> GetAllRecords()
        {
            var res = await _service.GetChatHistoryByID(await UserId);
            return Ok(res);
        }
        /// <summary>
        /// Delete All chat cards
        /// </summary>
        /// <returns></returns>
        [HttpDelete("/clear-chat-history")]
        [ProducesResponseType(typeof(ChatCardModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> DeleteAllChathistory()
        {
            Guid UserId = await _userService.GetIdByUserName(UserName);

            var res = await _service.DeleteAllChatList(UserId);
            return Ok(res);
        }
        /// <summary>
        /// Delete chat card By ID
        /// </summary>
        /// <returns></returns>
        [HttpDelete("/chathistory/{id}")]
        [ProducesResponseType(typeof(ChatCardModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> DeleteChatHistoryByID([FromRoute] string id)
        {
            Guid chatListID = Guid.Parse(id);

            var res = await _service.DeleteChatListByID(chatListID);
            return Ok("success");
        }
    }
}
