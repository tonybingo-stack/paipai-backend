using SignalRHubs.Entities;
using SignalRHubs.Models;

namespace SignalRHubs.Interfaces.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHomeService
    {


        Task<Guid> CreateCommunity(Community entity);
        Task<IEnumerable<CommunityViewModel>> GetCommunity(Guid id);
        Task<Guid> UpdateCommunity(Community entity);
        Task<Guid> DeleteCommunity(Guid id);
        Task<Guid> CreateChannel(Channel entity);
        Task<IEnumerable<ChannelViewModel>> GetAllChannels(Guid communityID);
        Task<Guid> UpdateChannel(ChannelUpdateModel model);
        Task<Guid> DeleteChannel(Guid id);

    }
}
