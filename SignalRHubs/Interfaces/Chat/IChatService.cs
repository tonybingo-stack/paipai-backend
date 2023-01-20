using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Interfaces.Services
{
    public interface IChatService
    {
        Task<IEnumerable<ChannelViewModel>> GetChatChannels(string username);
        Task<IEnumerable<ChannelMessageViewModel>> GetMessageByChannelId(Guid channelId, int offset);
        Task<MessageViewModel> GetMessage(Guid id);
        Task SaveMessage(Message entity);
        Task PutMessage(Message chatMessage);
        Task DeleteMessage(Guid id);
        Task<IEnumerable<ChatCardModel>> GetChatCards(string username);
        Task RefreshChatCard(string sender, string receiver);
        Task<IEnumerable<ChatModel>> GetChatHistory(ChatHistoryBindingModel model, int offset);
        Task<string> DeleteAllChatCards(string username); 
        Task<string> DeleteChatCardByID(Guid chatCardID);
        Task<ChannelMessage> GetChannelMessageById(Guid id);
        Task UpdateChannelMessage(ChannelMessage m);
        Task DeleteChannelMessageById(Guid messageId);
        Task SaveChannelMessage(ChannelMessage message);
    }
}