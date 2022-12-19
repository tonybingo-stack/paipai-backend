using AutoMapper;
using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Profiles
{
    public class Profiles: Profile
    {
        public Profiles()
        {
            CreateMap<User, ReadUserSummaryModel>();
            CreateMap<MessageBindingModel, Message>();
            CreateMap<Message, MessageViewModel>();
            CreateMap<ChatRoom, ChatRoomViewModel>();

        }
    }
}
