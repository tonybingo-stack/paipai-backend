using DbHelper.Interfaces.Services;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
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
                $"'{entity.CommunityId}', " +
                $"'{entity.CommunityName}', " +
                $"'{entity.CommunityDescription}', " +
                $"'{entity.CommunityOwnerId}', " +
                $"'{entity.CommunityType}'" +
                $")";

            await _service.GetDataAsync(query);

            return "Sucessfully created new community!";
        }
    }
}
