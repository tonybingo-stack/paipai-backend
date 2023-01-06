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


        Task<string> CreateCommunity(Community entity);
        Task<IEnumerable<CommunityViewModel>> GetCommunity(Guid id);
        Task<string> CreateChannel(Channel entity);
    }
}
