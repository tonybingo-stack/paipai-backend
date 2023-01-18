using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SignalRHubs.Entities;
using SignalRHubs.Hubs;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;
using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Controllers.Chat
{
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
        /// Get details of a message by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Return message details</returns>
        [ProducesResponseType(typeof(MessageViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpGet("/message/{id}")]
        public async Task<IActionResult> GetMessageById(string id)
        {
            return Ok(await _service.GetMessage(Guid.Parse(id)));
        }

        /// <summary>
        /// Send message 
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("/send-message")]
        public async Task<IActionResult> SendMessage([FromForm] MessageBindingModel model)
        {
            if (model.Content == null) return BadRequest("You can not send an empty message.");
            if (model.ReceiverUserName == null) return BadRequest("Receiver Name is required!");
            // Send message to receiver
            await _hubContext.Clients.User(model.ReceiverUserName).SendAsync("echo", UserName, model.Content);

            // Change message content for DB acceptable
            string content = model.Content;
            content = content.Replace("'", "''");

            // Save message
            Message message = _mapper.Map<Message>(model);
            message.SenderUserName = UserName;
            message.Content = content;
            message.ChannelId = model.ChannelId;
            message.FilePath = model.FilePath;
            await _service.SaveMessage(message);

            // Update ChatCard table by sender and receiver.
            ChatCardModel cardModel = new ChatCardModel();
            cardModel.SenderUserName = message.SenderUserName;
            cardModel.ReceiverUserName = message.ReceiverUserName;
            cardModel.Content = content;
            cardModel.isSend = true;
            cardModel.isDeleted = false;
            await _service.CreateOrUpdateChatCards(cardModel);

            cardModel.SenderUserName = message.ReceiverUserName;
            cardModel.ReceiverUserName = message.SenderUserName; 
            cardModel.Content = content;
            cardModel.isSend = false;
            cardModel.isDeleted = false;
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
        [HttpPut("/messages")]
        public async Task<IActionResult> Put([FromForm] MessageUpdateModel model)
        {
            Message message = _mapper.Map<Message>(await _service.GetMessage(model.Id));
            if (message == null) return BadRequest("Message does not exists.");

            if (model.Content != null) message.Content = model.Content.Replace("'", "''");
            message.FilePath = model.FilePath;
            message.UpdatedAt = DateTime.Now;
            message.EntityState = DbHelper.Enums.EntityState.Modified;

            await _service.PutMessage(message);
            //update chat card
            await _service.RefreshChatCard(message.SenderUserName, message.ReceiverUserName);
            //await _hubContext.Clients.All.SendAsync("notifyMessage", messageVm);
            message.Content = model.Content;

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
        [HttpDelete("/messages")]
        public async Task<IActionResult> Delete([FromForm][Required] string id)
        {
            Message message = _mapper.Map<Message>(await _service.GetMessage(Guid.Parse(id)));
            if (message == null) return BadRequest("Message does not exists.");
            message.Id = Guid.Parse(id);
            message.EntityState = DbHelper.Enums.EntityState.Deleted;
            
            await _service.DeleteMessage(Guid.Parse(id));

            //update chat card
            await _service.RefreshChatCard(message.SenderUserName, message.ReceiverUserName);
            //await _hubContext.Clients.User(UserId.ToString()).SendAsync("notifyDeleteMessage", messageVm);
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
        /// Get User by UserName
        /// </summary>
        /// <returns></returns>
        [HttpGet("/user/{username}")]
        [ProducesResponseType(typeof(ReadUserSummaryModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReadUserSummaryModel>> GetCustomerSummaryById([FromRoute] string username)
        {
            var user = await _userService.GetUserByUserName(username);
            var userSummary = _mapper.Map<ReadUserSummaryModel>(user);
            return Ok(userSummary);
        }

        /// <summary>
        /// Get messages of a channel by Id
        /// </summary>
        /// <param name="channelId">Chat Channel Id</param>
        /// <returns>List of chat messages.</returns>
        [ProducesResponseType(typeof(List<ChannelChatModel>), 200)]
        [ProducesResponseType(500)]
        [HttpGet("/channel/chat-history")]
        public async Task<IActionResult> GetMessagesByChannelId([FromQuery][Required] Guid channelId)
        {
            return Ok(await _service.GetMessageByChannelId(channelId));
        }

        /// <summary>
        /// Get Chat History of user between offset*10 ~ (offset+1)*10 messages
        /// </summary>
        /// <returns></returns>  
        [HttpGet("/chatcard/chat-history")]
        [ProducesResponseType(typeof(ChatModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ChatModel>>> GetChatHistory([FromQuery][Required] string receivername, [FromQuery][Required] int offset)
        {
            ChatHistoryBindingModel model = new ChatHistoryBindingModel();
            model.ReceiverUserName = receivername;
            model.SenderUserName = UserName;
            return Ok(await _service.GetChatHistory(model, offset));
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
            var res = await _service.GetChatCards(UserName);
            return Ok(res);
        }
        /// <summary>
        /// Delete All chat cards
        /// </summary>
        /// <returns></returns>
        [HttpDelete("/chatcards")]
        [ProducesResponseType(typeof(ChatCardModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> DeleteAllChathistory()
        {
            var res = await _service.DeleteAllChatCards(UserName);
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
