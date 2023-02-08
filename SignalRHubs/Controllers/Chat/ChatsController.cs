using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SignalRHubs.Entities;
using SignalRHubs.Hubs;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Lib;
using SignalRHubs.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SignalRHubs.Controllers.Chat
{
    public class ChatsController : ApiBaseController
    {
        private readonly IChatService _service;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions expiration;

        public ChatsController(IChatService service, IHubContext<ChatHub> hubContext
            , IMapper mapper
            , IUserService userService, IDistributedCache cache
            ) :base(userService)
        {
            _service = service;
            _hubContext = hubContext;
            _mapper = mapper;
            _userService = userService;
            _cache = cache;
            expiration = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1000),
                SlidingExpiration = TimeSpan.FromDays(100)
            };
        }

        private async Task<ChannelMessageViewModel> GetChannelMessageByMessageId(Guid channelId,Guid id)
        {
            List<ChannelMessage> data = new List<ChannelMessage>();
            ChannelMessageViewModel result = new ChannelMessageViewModel();

            string? serializedData = null;
            var dataAsByteArray = await _cache.GetAsync($"ChannelMessage:{channelId.ToString().Replace("-", "")}");
            if ((dataAsByteArray?.Count() ?? 0) > 0)
            {
                serializedData = Encoding.UTF8.GetString(dataAsByteArray);
                data = JsonConvert.DeserializeObject<List<ChannelMessage>>(serializedData);
            }
            ChannelMessage cm = data.Find(x => x.Id == id);
            if (cm == null)
            {
                // Fetch from db
                cm = await _service.GetChannelMessageById(id);

            }
            if (cm == null) return null;

            result = _mapper.Map<ChannelMessageViewModel>(cm);
            // if Replied message, we have to know about the user
            if (result.RepliedTo != null)
            {
                // find being replied message.
                ChannelMessage? m = data.Find(x => x.Id == result.RepliedTo);
                if (m == null)
                {
                    // if not exist in cache, we have to fetch from DB
                    m = await _service.GetChannelMessageById(result.RepliedTo);
                }
                result.RepliedContent = m.Content;
                result.RepliedUserName = m.SenderUserName;
                result.RepliedMsgCreatedAt = m.CreatedAt;
                // User Avatar
                User u = await _userService.GetUserByUserName(m.SenderUserName);
                result.RepliedUserAvatar = u.Avatar;
            }

            return result;

        }

        /// <summary>
        /// Get Chat History of channel between offset*10 ~ (offset+1)*10 messages
        /// </summary>
        [ProducesResponseType(typeof(List<ChannelMessageViewModel>), 200)]
        [ProducesResponseType(500)]
        [HttpGet("/channel/messages")]
        public async Task<ActionResult<IEnumerable<ChannelMessageViewModel>>> GetMessagesByChannelId([FromQuery][Required] Guid channelId, [FromQuery][Required] int offset)
        {
            List<ChannelMessage> data = new List<ChannelMessage>();
            List<ChannelMessage> data_new = new List<ChannelMessage>();
            List<ChannelMessageViewModel> results = new List<ChannelMessageViewModel>();

            if (offset < 5)
            {
                string? serializedData = null;
                var dataAsByteArray = await _cache.GetAsync($"ChannelMessage:{channelId.ToString().Replace("-", "")}");
                if ((dataAsByteArray?.Count() ?? 0) > 0)
                {
                    serializedData = Encoding.UTF8.GetString(dataAsByteArray);
                    data = JsonConvert.DeserializeObject<List<ChannelMessage>>(serializedData);
                }
                data_new = data.GetRange(offset * 10, 10 > (data.Count - offset * 10) ? (data.Count - offset * 10) : 10);
                foreach (var item in data_new)
                {
                    ChannelMessageViewModel channelMsg = _mapper.Map<ChannelMessageViewModel>(item);
                    // if Replied message, we have to know about the user
                    if (item.RepliedTo != null)
                    {
                        // find being replied message.
                        ChannelMessage? m = data.Find(x => x.Id == item.RepliedTo);
                        if (m == null)
                        {
                            // if not exist in cache, we have to fetch from DB
                            m = await _service.GetChannelMessageById(item.RepliedTo);
                        }
                        channelMsg.RepliedContent = m.Content;
                        channelMsg.RepliedUserName = m.SenderUserName;
                        channelMsg.RepliedMsgCreatedAt = m.CreatedAt;
                        // Avatar setting
                        // User Avatar
                        User u = await _userService.GetUserByUserName(m.SenderUserName);
                        channelMsg.RepliedUserAvatar = u.Avatar;
                    }

                    results.Add(channelMsg);
                }
                return Ok(results);
            }
            else
            {
                // Have to Fetch from db
                return Ok(await _service.GetMessageByChannelId(channelId, offset - 5));
            }
        }
        /// <summary>
        /// Send(Reply) message To Channel
        /// </summary>
        [ProducesResponseType(typeof(Guid),200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("/channel/message")]
        public async Task<IActionResult> SendMessageToChannel([FromForm] ChannelSendMessageModel model)
        {
            if (model.Content == null && model.FilePath == null) return BadRequest("You can not send an empty message.");
            if (model.Content != null && model.FilePath != null) return BadRequest("You can't send message and file at samee time");
            //if (!((model.FilePath == null) ^ (model.FileType == null) ^ (model.FilePreviewW == null) ^ (model.FilePreviewH == null))) return BadRequest("Bad Request");

            ChannelMessage message = new ChannelMessage();
            message.Id = Guid.NewGuid();
            message.SenderUserName = UserName;
            message.ChannelId = model.ChannelId;
            message.Content = model.Content;
            message.FilePath = model.FilePath;
            message.FileType = model.FileType;
            message.FilePreviewW = model.FilePreviewW;
            message.FilePreviewH = model.FilePreviewH;
            message.RepliedTo = model.RepiedTo;
            message.isDeleted = false;
            message.CreatedAt = DateTime.Now;
            message.UpdatedAt = null;
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
            // save message to cache first 
            List<ChannelMessage> data = new List<ChannelMessage>();
            string ? serializedData = null;
            var dataAsByteArray = await _cache.GetAsync($"ChannelMessage:{model.ChannelId.ToString().Replace("-", "")}");
            if ((dataAsByteArray?.Count() ?? 0) > 0)
            {
                serializedData = Encoding.UTF8.GetString(dataAsByteArray);
                data = JsonConvert.DeserializeObject<List<ChannelMessage>>(serializedData);
            }
            if(data.Count >=50)
            {
                // Move data to DB
                await _service.SaveChannelMessage(data);
                // Init data
                data.Clear();
            }
            data.Insert(0,message);
            serializedData = JsonConvert.SerializeObject(data);
            dataAsByteArray = Encoding.UTF8.GetBytes(serializedData);
            await _cache.SetAsync($"ChannelMessage:{model.ChannelId.ToString().Replace("-", "")}", dataAsByteArray);

            // Send message to receiver
            ChannelMessageViewModel? m = await GetChannelMessageByMessageId(message.ChannelId, message.Id);
            if (m== null) return BadRequest("Something went wrong!");

            string content = JsonConvert.SerializeObject(m);
            Console.WriteLine("channel message: " + content);
            await _hubContext.Clients.Group(model.ChannelId.ToString()).SendAsync("echo", UserName, content);
            return Ok(message.Id);
        }
        /// <summary>
        /// Update message of channel
        /// </summary>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPut("/channel/message")]
        public async Task<IActionResult> UpdateMessageOfChannel([FromForm] ChannelMessageUpdateModel model)
        {
            if (model.Content == null && model.FilePath == null) return BadRequest("At least one field is required!");
            // Check if message exist in cache
            List<ChannelMessage>? data = new List<ChannelMessage>();
            string? serializedData = null;
            var dataAsByteArray = await _cache.GetAsync($"ChannelMessage:{model.ChannelId.ToString().Replace("-", "")}");
            if ((dataAsByteArray?.Count() ?? 0) > 0)
            {
                serializedData = Encoding.UTF8.GetString(dataAsByteArray);
                data = JsonConvert.DeserializeObject<List<ChannelMessage>>(serializedData);
            }
            ChannelMessage? m = data?.Find(x=> x.Id == model.Id);
            if (m != null)
            {
                if (model.Content != null) m.Content = model.Content;
                if (model.FilePath != null) m.FilePath = model.FilePath;
                if (model.FileType != null) m.FileType = model.FileType;
                if (model.FilePreviewW != null) m.FilePreviewW = model.FilePreviewW;
                if (model.FilePreviewH != null) m.FilePreviewH = model.FilePreviewH;

                int index = data.IndexOf(m);
                data[index] = m;
                // set to cache
                serializedData = JsonConvert.SerializeObject(data);
                dataAsByteArray = Encoding.UTF8.GetBytes(serializedData);
                await _cache.SetAsync($"ChannelMessage:{model.ChannelId.ToString().Replace("-", "")}", dataAsByteArray);
            }
            else
            {   
                m = await _service.GetChannelMessageById(model.Id);
                if (m == null) return BadRequest("Message doesn't exist!");      
                
                if (model.Content != null) m.Content = model.Content;
                if (model.FilePath != null) m.FilePath = model.FilePath;
                if (model.FileType != null) m.FileType = model.FileType;
                if (model.FilePreviewW != null) m.FilePreviewW = model.FilePreviewW;
                if (model.FilePreviewH != null) m.FilePreviewH = model.FilePreviewH;

                await _service.UpdateChannelMessage(m);
            }

            return Ok("Message updated successfully");
        }

        /// <summary>
        /// Delete message of channel
        /// </summary>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpDelete("/channel/message")]
        public async Task<IActionResult> DeleteMessageOfChannel([Required][FromQuery]Guid messageId, [Required][FromQuery]Guid channelId)
        {
            // Check if message exist in cache
            List<ChannelMessage>? data = new List<ChannelMessage>();
            string? serializedData = null;
            var dataAsByteArray = await _cache.GetAsync($"ChannelMessage:{channelId.ToString().Replace("-", "")}");
            if ((dataAsByteArray?.Count() ?? 0) > 0)
            {
                serializedData = Encoding.UTF8.GetString(dataAsByteArray);
                data = JsonConvert.DeserializeObject<List<ChannelMessage>>(serializedData);
            }
            ChannelMessage? m = data?.Find(x => x.Id == messageId);
            if( m !=null)
            {
                data.Remove(m);
                // set to cache
                serializedData = JsonConvert.SerializeObject(data);
                dataAsByteArray = Encoding.UTF8.GetBytes(serializedData);
                await _cache.SetAsync($"ChannelMessage:{channelId.ToString().Replace("-", "")}", dataAsByteArray);
            }
            else
            {
                m = await _service.GetChannelMessageById(messageId);
                if (m == null) return BadRequest("Message doesn't exist!");
                await _service.DeleteChannelMessageById(messageId);
            }

            return Ok("Message deleted successfully");
        }
        
        private async Task<ChatModel> GetMessageById(Guid Id, string receivername)
        {
            List<Message>? data = new List<Message>();
            ChatModel result = new ChatModel();

            string encode = Utils.Base64Encode(UserName, receivername);

            string? serializedData = null;
            var dataAsByteArray = await _cache.GetAsync($"Message:" + encode);
            if ((dataAsByteArray?.Count() ?? 0) > 0)
            {
                serializedData = Encoding.UTF8.GetString(dataAsByteArray);
                data = JsonConvert.DeserializeObject<List<Message>>(serializedData);
            }
            Message? message = data.Find(x => x.Id == Id);

            if (message == null)
            {
                // Have to fetch from sql db
                message = await _service.GetMessage(Id);
            }
            if (message == null) return null;

            result = _mapper.Map<ChatModel>(message);
            if (result.RepliedTo != null)
            {
                // find being replied message.
                Message? m = data.Find(x => x.Id == result.RepliedTo);
                if (m == null)
                {
                    // if not exist in cache, we have to fetch from DB
                    m = await _service.GetMessage(result.RepliedTo);
                }
                result.RepliedContent = m.Content;
                result.RepliedUserName = m.SenderUserName;
                result.RepliedMsgCreatedAt = m.CreatedAt;
                // User Avatar
                User u = await _userService.GetUserByUserName(m.SenderUserName);
                result.RepliedUserAvatar = u.Avatar;
            }
            return result;
        }
        /// <summary>
        /// Get Chat History of user between offset*10 ~ (offset+1)*10 messages
        /// </summary>
        /// <returns></returns>  
        [HttpGet("/chatcard/messages")]
        [ProducesResponseType(typeof(List<ChatModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ChatModel>>> GetChatHistory([FromQuery][Required] string receivername, [FromQuery][Required] int offset)
        {
            List<Message>? data = new List<Message>();
            List<Message>? data_new = new List<Message>();
            List<ChatModel> results = new List<ChatModel>();
            string encode = Utils.Base64Encode(UserName, receivername);

            string? serializedData = null;
            var dataAsByteArray = await _cache.GetAsync($"Message:"+encode);
            if ((dataAsByteArray?.Count() ?? 0) > 0)
            {
                serializedData = Encoding.UTF8.GetString(dataAsByteArray);
                data = JsonConvert.DeserializeObject<List<Message>>(serializedData);
            }

            if ((data.Count - offset * 10)>0)
            {
                data_new = data?.GetRange(offset * 10, 10>(data.Count-offset*10)? (data.Count - offset * 10) : 10 );
                foreach (var item in data_new)
                {
                    ChatModel chatModel = _mapper.Map<ChatModel>(item);

                    // if Replied message, we have to know about the user
                    if (item.RepliedTo != null)
                    {
                        // find being replied message.
                        Message? m = data.Find(x => x.Id == item.RepliedTo);
                        if (m == null)
                        {
                            // if not exist in cache, we have to fetch from DB
                            m = await _service.GetMessage(item.RepliedTo);
                        }
                        chatModel.RepliedContent = m.Content;
                        chatModel.RepliedUserName = m.SenderUserName;
                        chatModel.RepliedMsgCreatedAt = m.CreatedAt;
                        // User Avatar
                        User u = await _userService.GetUserByUserName(m.SenderUserName);
                        chatModel.RepliedUserAvatar = u.Avatar;
                    }

                    results.Add(chatModel);
                }
                return Ok(results);
            }
            else
            {
                // Have to Fetch from db
                ChatHistoryBindingModel model = new ChatHistoryBindingModel();
                model.ReceiverUserName = receivername;
                model.SenderUserName = UserName;

                return Ok(await _service.GetChatHistory(model, Convert.ToInt32((offset * 10 - data.Count) / 10)));
            }
        }
        /// <summary>
        /// Send(Reply) message 
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("/chatcard/message")]
        public async Task<IActionResult> SendMessage([FromForm] MessageBindingModel model)
        {
            if (model.Content == null && model.FilePath == null) return BadRequest("You can not send an empty message.");
            if (model.ReceiverUserName == null) return BadRequest("Receiver Name is required!");
            if (model.Content != null && model.FilePath != null) return BadRequest("You can't send message and file at same time");
            //if (!((model.FilePath == null) ^ (model.FileType == null) ^ (model.FilePreviewW == null) ^ (model.FilePreviewH == null))) return BadRequest("Bad Request");
            // is friend? is blocked?
            string _status = await _service.CheckUserFriendShip(UserName, model.ReceiverUserName);
            if (_status == null) return BadRequest($"You can't send message until you accept invitation.");
            if (_status == "blocked") return BadRequest($"You are blocked by this user.");

            // create message
            Message message = _mapper.Map<Message>(model);
            message.Id = Guid.NewGuid();
            message.SenderUserName = UserName;
            message.isDeleted = false;
            string encode = Utils.Base64Encode(UserName, model.ReceiverUserName);
            // save message to cache first 
            List<Message> data = new List<Message>();
            string? serializedData = null;
            var dataAsByteArray = await _cache.GetAsync($"Message:"+encode);
            if ((dataAsByteArray?.Count() ?? 0) > 0)
            {
                serializedData = Encoding.UTF8.GetString(dataAsByteArray);
                data = JsonConvert.DeserializeObject<List<Message>>(serializedData);
            }

            if (data.Count >= 50)
            {
                // Move data to DB
                await _service.SaveMessage(data);
                // Init data
                data.Clear();
            }
            data.Insert(0, message);
            serializedData = JsonConvert.SerializeObject(data);
            dataAsByteArray = Encoding.UTF8.GetBytes(serializedData);
            await _cache.SetAsync($"Message:"+encode, dataAsByteArray);

            // Update chatcard cache
            await _service.RefreshChatCard(UserName, model.ReceiverUserName, message);
            // Send message to  receiver
            ChatModel? c = await GetMessageById(message.Id, model.ReceiverUserName);

            if (c == null) return BadRequest("Something went wrong!");

            string content = JsonConvert.SerializeObject(c);
            Console.WriteLine("DM: "+content);
            await _hubContext.Clients.User(model.ReceiverUserName).SendAsync("echo", UserName, content);

            return Ok(message.Id);
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
        [HttpPut("/chatcard/message")]
        public async Task<IActionResult> Put([FromForm] MessageUpdateModel model)
        {
            if (model.FilePath == null && (model.FileType != null || model.FilePreviewW != null || model.FilePreviewH != null)) return BadRequest("Bad request");
            if (model.FilePath != null && (model.FileType == null || model.FilePreviewW == null || model.FilePreviewH == null)) return BadRequest("Bad request");
            // Get message from Cache
            List<Message>? data = new List<Message>();
            string encode = Utils.Base64Encode(UserName, model.ReceiverUserName);

            string? serializedData = null;
            var dataAsByteArray = await _cache.GetAsync($"Message:"+encode);
            if ((dataAsByteArray?.Count() ?? 0) > 0)
            {
                serializedData = Encoding.UTF8.GetString(dataAsByteArray);
                data = JsonConvert.DeserializeObject<List<Message>>(serializedData);
            }
            Message? message = data?.Find(x=> x.Id == model.Id);
            if(message != null)
            {
                int index = data.IndexOf(message);
                message.Content = model.Content;
                message.FilePath = model.FilePath;
                message.FileType = model.FileType;
                message.FilePreviewW = model.FilePreviewW;
                message.FilePreviewH = model.FilePreviewH;
                message.UpdatedAt = DateTime.Now;
                message.EntityState = DbHelper.Enums.EntityState.Modified;

                data[index] = message;
                // Reset to Cache
                serializedData = JsonConvert.SerializeObject(data);
                dataAsByteArray = Encoding.UTF8.GetBytes(serializedData);
                await _cache.SetAsync($"Message:" + encode, dataAsByteArray);
                
                if(index ==0) await _service.RefreshChatCard(UserName, model.ReceiverUserName, message);
            }
            else
            {
                message = await _service.GetMessage(model.Id);
                if (message == null) return BadRequest("Message does not exists.");
                message.Content = model.Content;
                message.FilePath = model.FilePath;
                message.FileType = model.FileType;
                message.FilePreviewW = model.FilePreviewW;
                message.FilePreviewH = model.FilePreviewH;
                message.UpdatedAt = DateTime.Now;
                message.EntityState = DbHelper.Enums.EntityState.Modified;

                await _service.PutMessage(message);
            }
            //await _hubContext.Clients.All.SendAsync("notifyMessage", messageVm);

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
        [HttpDelete("/chatcard/message")]
        public async Task<IActionResult> Delete([FromForm][Required] Guid id, [Required][FromForm] string receiverUserName)
        {
            // Get message from Cache
            List<Message>? data = new List<Message>();
            string encode = Utils.Base64Encode(UserName, receiverUserName);

            string? serializedData = null;
            var dataAsByteArray = await _cache.GetAsync($"Message:" + encode);
            if ((dataAsByteArray?.Count() ?? 0) > 0)
            {
                serializedData = Encoding.UTF8.GetString(dataAsByteArray);
                data = JsonConvert.DeserializeObject<List<Message>>(serializedData);
            }
            Message? message = data?.Find(x => x.Id == id);
            if(message!=null)
            {
                int index = data.IndexOf(message);

                if(index ==0) await _service.RefreshChatCard(UserName, receiverUserName, message);
                data.Remove(message);
                // Reset to Cache
                serializedData = JsonConvert.SerializeObject(data);
                dataAsByteArray = Encoding.UTF8.GetBytes(serializedData);
                await _cache.SetAsync($"Message:" + encode, dataAsByteArray);
            }
            else
            {
                message = await _service.GetMessage(id);
                if (message == null) return BadRequest("Message does not exists.");
                message.EntityState = DbHelper.Enums.EntityState.Deleted;

                await _service.DeleteMessage(id);
            }
            //await _hubContext.Clients.User(UserId.ToString()).SendAsync("notifyDeleteMessage", messageVm);
            return Ok(message);
        }

        /// <summary>
        /// Get All Chat cards
        /// </summary>
        /// <returns></returns>
        [HttpGet("/chatcards")]
        [ProducesResponseType(typeof(ChatCardViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ChatCardViewModel>>> GetAllChatCards()
        {
            var res = await _service.GetChatCards(UserName);
            return Ok(res);
        }
        /// <summary>
        /// Delete All chat cards
        /// </summary>
        /// <returns></returns>
        [HttpDelete("/chatcards")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> DeleteChatCardByID([FromRoute] string id)
        {
            Guid chatCardID = Guid.Parse(id);

            var res = await _service.DeleteChatCardByID(chatCardID);
            return Ok("success");
        }
        /// <summary>
        /// User Typing
        /// </summary>
        /// <returns></returns>
        [HttpPost("/type")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> Type([FromForm][Required]string username, [FromForm][Required] bool isTyping)
        {
            await _hubContext.Clients.User(username).SendAsync("Typing", UserName, isTyping);
            return Ok("success"); 
        }
    }
}