using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Interfaces.Services
{
    public interface IChatService
    {
        Task<IEnumerable<ChannelViewModel>> GetChatChannels(string username);
        Task<IEnumerable<ChannelChatModel>> GetMessageByChannelId(Guid channelId);
        Task<MessageViewModel> GetMessage(Guid id);
        Task SaveMessage(Message chatMessage);
        Task PutMessage(Message chatMessage);
        Task DeleteMessage(Guid id);
        Task SaveChannel(Channel _channel);
        Task CreateOrUpdateChatCards(ChatCardModel model);
        Task<IEnumerable<ChatCardModel>> GetChatCards(string username);
        Task RefreshChatCard(string sender, string receiver);
        Task<IEnumerable<ChatModel>> GetChatHistory(ChatHistoryBindingModel model, int offset);
        Task<string> DeleteAllChatCards(string username); 
        Task<string> DeleteChatCardByID(Guid chatCardID); 
    }
}