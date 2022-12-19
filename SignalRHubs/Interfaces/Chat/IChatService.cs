using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Interfaces.Services
{
    public interface IChatService
    {
        Task<List<ChatRoomViewModel>> GetChatRoomsByUserId(Guid userId);
        Task<Tuple<Guid?, IEnumerable<string>>> GetChatRoomByUserIds(IEnumerable<string> userIds);
        Task<List<MessageViewModel>> GetMessageByRoomId(Guid roomId);
        Task<Message> GetMessage(Guid id);
        Task SaveMessage(Message chatMessage);
        Task SaveRoom(ChatRoom room);
    }
}