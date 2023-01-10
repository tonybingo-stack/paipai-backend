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
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("/send-message")]
        public async Task<IActionResult> SendMessage([FromForm] MessageBindingModel model)
        {
            Guid senderID = await UserId;
            var sender = await _userService.GetUserByID(senderID);
            var receiver = await _userService.GetUserByID(model.ReceiverId);
            //Send message to receiver
            await _hubContext.Clients.User(receiver.UserName).SendAsync("echo", sender.UserName, model.Content);

            Message message = _mapper.Map<Message>(model);
            message.Id = Guid.NewGuid();
            message.ChannelId = model.ChannelId;
            message.SenderId = senderID;

            await _service.SaveMessage(message);

            // Update ChatHistory table by sender and receiver.
            ChatCardModel cardModel = new ChatCardModel();
            cardModel.UserID = message.SenderId;
            cardModel.ReceiverId = message.ReceiverId;
            cardModel.Content = message.Content;
            cardModel.isSend = true;
            cardModel.isDeleted = false;
            cardModel.NickName = receiver.NickName;
            cardModel.Avatar = receiver.Avatar;
            await _service.CreateOrUpdateChatCards(cardModel);

            cardModel.UserID =  message.ReceiverId;
            cardModel.ReceiverId = message.SenderId;
            cardModel.Content = message.Content;
            cardModel.isSend = false;
            cardModel.isDeleted = false;
            cardModel.NickName = sender.NickName;
            cardModel.Avatar = sender.Avatar;
            await _service.CreateOrUpdateChatCards(cardModel);

            return Ok("success");
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
        /// Get UserID by username
        /// </summary>
        /// <returns></returns>
        [HttpGet("/user/userID/{name}")]
        public async Task<ActionResult<Guid>> GetUserIDByName([FromRoute] string name)
        {
            return Ok(await _userService.GetIdByUserName(name));
        }
        /// <summary>
        /// Get Chat History of user
        /// </summary>
        /// <returns></returns>  
        [HttpPost("/chathistory")]
        [ProducesResponseType(typeof(ChatModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ChatModel>>> GetChatHistory([FromForm] Guid ReceverID)
        {
            ChatHistoryBindingModel model = new ChatHistoryBindingModel();
            model.ReceiverID = ReceverID;
            model.SenderID = await UserId;
            return Ok(await _service.GetChatHistory(model));
        }
        /// <summary>
        /// Get All Chat cards
        /// </summary>
        /// <returns></returns>
        [HttpGet("/chatcards")]
        [ProducesResponseType(typeof(ChatCardModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ChatCardModel>>> GetAllChatCards()
        {
            var res = await _service.GetChatCards(await UserId);
            return Ok(res);
        }
        /// <summary>
        /// Delete All chat cards
        /// </summary>
        /// <returns></returns>
        [HttpDelete("/clear-chat-card")]
        [ProducesResponseType(typeof(ChatCardModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> DeleteAllChathistory()
        {
            var res = await _service.DeleteAllChatCards(await UserId);
            return Ok(res);
        }
        /// <summary>
        /// Delete chat card By ID
        /// </summary>
        /// <returns></returns>
        [HttpDelete("/chatcard/{id}")]
        [ProducesResponseType(typeof(ChatCardModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> DeleteChatCardByID([FromRoute] string id)
        {
            Guid chatCardID = Guid.Parse(id);

            var res = await _service.DeleteChatCardByID(chatCardID);
            return Ok("success");
        }
    }
}
