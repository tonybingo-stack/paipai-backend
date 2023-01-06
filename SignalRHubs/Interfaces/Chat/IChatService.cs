using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Interfaces.Services
{
    public interface IChatService
    {
        Task<List<ChatChannelViewModel>> GetChatChannelsByUserId(Guid userId);
        Task<List<MessageViewModel>> GetMessageByChannelId(Guid channelId);
        Task<MessageViewModel> GetMessage(Guid id);
        Task SaveMessage(Message chatMessage);
        Task PutMessage(Guid id, Message chatMessage);
        Task DeleteMessage(Guid id);
        Task SaveChannel(Channel _channel);
        Task<Tuple<Guid?, IEnumerable<string>>> GetChatChannelByUserIds(IEnumerable<string> userIds);
        Task CreateOrUpdateChatCards(ChatCardModel model);
        Task<List<ChatCardModel>> GetChatCards(Guid userId);
        Task<List<ChatModel>> GetChatHistory(ChatHistoryBindingModel model);
        Task<string> DeleteAllChatCards(Guid userId); 
        Task<string> DeleteChatCardByID(Guid chatListID); 
    }
}