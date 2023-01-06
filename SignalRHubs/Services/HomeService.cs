using DbHelper.Interfaces.Services;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;
using System.Data.SqlClient;

namespace SignalRHubs.Services
{
    public class HomeService:IHomeService
    {
        private readonly IDapperService<Community> _service;
        private readonly SqlConnection _connection;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repository"></param>
        public HomeService(IDapperService<Community> dapperService)
        {
            _service = dapperService;
            _connection = dapperService.Connection;
        }

        public async Task<string> CreateChannel(Channel entity)
        {
            var query = $"INSERT INTO Channel VALUES(" +
                $"'{entity.ChannelId}', " +
                $"'{entity.ChannelName}', " +
                $"'{entity.ChannelDescription}', " +
                $"'{entity.ChannelCommunityId}' " +
                $")";

            await _service.GetDataAsync(query);

            return "Sucessfully created new channel!";
        }

        /// <summary>
        /// Create new community
        /// </summary>
        /// <param name="community"></param>
        /// <returns></returns>
        public async Task<string> CreateCommunity(Community entity)
        {
            var query = $"INSERT INTO Community VALUES(" +
                $"'{entity.Id}', " +
                $"'{entity.CommunityName}', " +
                $"'{entity.CommunityDescription}', " +
                $"'{entity.CommunityOwnerId}', " +
                $"'{entity.CommunityType}'" +
                $"'{entity.CreatedAt}'" +
                $"'{entity.UpdatedAt}'" +
                $")";

            await _service.GetDataAsync(query);

            return "Sucessfully created new community!";
        }

        public async Task<IEnumerable<CommunityViewModel>> GetCommunity(Guid id)
        {
            var query = $"SELECT * FROM dbo.Community WHERE dbo.Community.CommunityOwnerId ='{id}'";

            var response = await _service.GetDataAsync<CommunityViewModel>(query);
            
            return response.ToList();
        }
    }
}
