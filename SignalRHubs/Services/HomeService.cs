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

        /// <summary>
        /// Create new community
        /// </summary>
        /// <param name="community"></param>
        /// <returns></returns>
        public async Task<string> CreateCommunity(Community community)
        {
            var query = $"INSERT INTO Community VALUES(" +
                $"{community.CommunityId}, " +
                $"'{community.CommunityName}', " +
                $"'{community.CommunityDescription}', " +
                $"{community.CommunityOwnerId}, " +
                $"'{community.CommunityType}'" +
                $")";

            await _service.GetDataAsync(query);

            return "Sucessfully created new community!";
        }
    }
}
