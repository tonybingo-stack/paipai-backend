using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Interfaces.Services
{
    public interface IChatService
    {
        Task<IEnumerable<ChannelViewModel>> GetChatChannels(string username);
        Task<IEnumerable<ChannelMessageViewModel>> GetMessageByChannelId(Guid channelId, int offset);
        Task<Message> GetMessage(Guid? id);
        Task SaveMessage(List<Message> messages);
        Task PutMessage(Message chatMessage);
        Task DeleteMessage(Guid id);
        Task<IEnumerable<ChatCardViewModel>> GetChatCards(string username);
        Task RefreshChatCard(string sender, string receiver, Message message);
        Task<IEnumerable<ChatModel>> GetChatHistory(ChatHistoryBindingModel model, int offset);
        Task<string> DeleteAllChatCards(string username); 
        Task<string> DeleteChatCardByID(Guid chatCardID);
        Task<ChannelMessage> GetChannelMessageById(Guid? id);
        Task UpdateChannelMessage(ChannelMessage m);
        Task DeleteChannelMessageById(Guid messageId);
        Task SaveChannelMessage(ChannelMessage message);
        Task<int> CheckUserFriendShip(string userName, string receiverUserName);
    }
}