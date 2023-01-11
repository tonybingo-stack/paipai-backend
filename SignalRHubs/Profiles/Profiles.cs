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
            CreateMap<MessageViewModel, Message>();
            CreateMap<ChatRoom, ChatChannelViewModel>();

            //CreateMap<UserModel, LoginCredential > ();
            CreateMap<CreateUserModel, User>();
            CreateMap<CommunityModel, Community>();
            CreateMap<CommunityUpdateModel, Community>();
            CreateMap<ChannelModel, Channel>();
            CreateMap<User, UserLoginModel>();
        }
    }
}
