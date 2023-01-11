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

        public async Task<Guid> CreateChannel(Channel entity)
        {
            var query = $"INSERT INTO Channel VALUES(" +
                $"'{entity.ChannelId}', " +
                $"'{entity.ChannelName}', " +
                $"'{entity.ChannelDescription}', " +
                $"'{entity.ChannelCommunityId}', " +
                $"CURRENT_TIMESTAMP," +
                $"null" +
                $")";

            await _service.GetDataAsync(query);

            return entity.ChannelId;
        }

        /// <summary>
        /// Create new community
        /// </summary>
        /// <param name="community"></param>
        /// <returns></returns>
        public async Task<Guid> CreateCommunity(Community entity)
        {
            var query = $"INSERT INTO Community VALUES(" +
                $"'{entity.Id}', " +
                $"'{entity.CommunityName}', " +
                $"'{entity.CommunityDescription}', " +
                $"'{entity.CommunityOwnerName}', " +
                $"'{entity.CommunityType}'," +
                $"CURRENT_TIMESTAMP," +
                $"NULL," +
                $"'{entity.ForegroundImage}'," +
                $"'{entity.BackgroundImage}'" +
                $")";

            await _service.GetDataAsync(query);

            return entity.Id;
        }

        public async Task<string> UpdateCommunityAvatar(Guid id, string url)
        {
            var query = $"UPDATE [dbo].[Community] SET ForegroundImage = '{url}' WHERE ID = '{id}'";
            await _service.GetDataAsync(query);
            return "Successfully updated community avatar!";
        }

        public async Task<string> UpdateCommunityBackGround(Guid id, string url)
        {
            var query = $"UPDATE [dbo].[Community] SET BackgroundImage = '{url}' WHERE ID = '{id}'";
            await _service.GetDataAsync(query);
            return "Successfully updated community background!";
        }

        public async Task<Guid> DeleteChannel(Guid id)
        {
            var query = $"DELETE FROM dbo.Channel WHERE ChannelId = '{id}';";
            await _service.GetDataAsync(query);
            return id;
        }

        public async Task<Guid> DeleteCommunity(Guid id)
        {
            var query = $"DELETE FROM dbo.Community WHERE ID = '{id}';";
            await _service.GetDataAsync(query);
            return id;
        }

        public async Task<IEnumerable<ChannelViewModel>> GetAllChannels(Guid communityID)
        {
            var query = $"SELECT * FROM dbo.Channel WHERE ChannelCommunityId = '{communityID}';";
            var response = await _service.GetDataAsync<ChannelViewModel>(query);
            return response.ToList();
        }

        public async Task<IEnumerable<CommunityViewModel>> GetCommunity(string username)
        {
            var query = $"SELECT * FROM dbo.Community WHERE dbo.Community.CommunityOwnerName ='{username}'";
            var response = await _service.GetDataAsync<CommunityViewModel>(query);
            
            return response.ToList();
        }

        public async Task<Guid> UpdateChannel(ChannelUpdateModel model)
        {
            var query = $"UPDATE dbo.Channel " +
                $"SET ChannelName='{model.ChannelName}',ChannelDescription = '{model.ChannelDescription}', UpdatedAt = CURRENT_TIMESTAMP " +
                $"WHERE dbo.Channel.ChannelId = '{model.ChannelId}'; ";

            var response = await _service.GetDataAsync(query);
            return model.ChannelId;
        }

        public async Task<Guid> UpdateCommunity(Community entity)
        {
            var query = $"UPDATE dbo.Community " +
                $"SET CommunityName='{entity.CommunityName}',CommunityDescription = '{entity.CommunityDescription}', CommunityType = '{entity.CommunityType}', UpdatedAt = CURRENT_TIMESTAMP, ForegroundImage = '{entity.ForegroundImage}', BackgroundImage = '{entity.BackgroundImage}' " +
                $"WHERE dbo.Community.ID = '{entity.Id}'; ";

            await _service.GetDataAsync(query);

            return entity.Id;
        }
    }
}
