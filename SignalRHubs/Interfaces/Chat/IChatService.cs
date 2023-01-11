using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Interfaces.Services
{
    public interface IChatService
    {
        Task<IEnumerable<ChatChannelViewModel>> GetChatChannels(string username);
        Task<IEnumerable<MessageViewModel>> GetMessageByChannelId(Guid channelId);
        Task<MessageViewModel> GetMessage(Guid id);
        Task SaveMessage(Message chatMessage);
        Task PutMessage(Message chatMessage);
        Task DeleteMessage(Guid id);
        Task SaveChannel(Channel _channel);
        Task<Tuple<Guid?, IEnumerable<string>>> GetChatChannelByUserIds(IEnumerable<string> userIds);
        Task CreateOrUpdateChatCards(ChatCardModel model);
        Task<IEnumerable<ChatCardModel>> GetChatCards(string username);
        Task<IEnumerable<ChatModel>> GetChatHistory(ChatHistoryBindingModel model, int offset);
        Task<string> DeleteAllChatCards(string username); 
        Task<string> DeleteChatCardByID(Guid chatCardID); 
    }
}