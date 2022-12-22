using Dapper;
using DbHelper.Interfaces.Services;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;
using System.Data.SqlClient;

namespace SignalRHubs.Services
{
    public class ChatService : IChatService
    {
        private readonly IDapperService<Message> _service;
        private readonly SqlConnection _connection;

        public ChatService(IDapperService<Message> service)
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<List<ChatChannelViewModel>> GetChatChannelsByUserId(Guid userId)
        {
            var query = $"SELECT * FROM dbo.Channel " +
                $"INNER JOIN dbo.Community ON dbo.Channel.ChannelCommunityId =dbo.Community.CommunityId " +
                $"INNER JOIN dbo.Users ON dbo.Community.CommunityOwnerId =dbo.Users.ID " +
                $"WHERE dbo.Users.ID =@UserId";
            return await _service.GetDataAsync<ChatChannelViewModel>(query, new {UserId = userId});
        }

        public async Task<MessageViewModel> GetMessage(Guid Id)
        {
            var query = $"SELECT " +
                $"dbo.Message.SenderID," +
                $"dbo.Message.ReceiverID," +
                $"dbo.Message.Content," +
                $"dbo.Message.ChannelID," +
                $"dbo.Message.FilePath " +
                $"FROM dbo.Message " +
                $"WHERE dbo.Message.ID =@Id";

            var response = await _service.GetDataAsync<MessageViewModel>(query, new { Id = Id });
            if (response == null) return null;
            else return response[0];
        }

        public async Task<List<MessageViewModel>> GetMessageByChannelId(Guid channelId)
        {
            var query = $"SELECT " +
                $"dbo.Message.SenderID," +
                $"dbo.Message.ReceiverID," +
                $"dbo.Message.Content," +
                $"dbo.Message.ChannelID," +
                $"dbo.Message.FilePath " +
                $"FROM dbo.Message " +
                $"INNER JOIN dbo.Channel ON dbo.Message.ChannelID =dbo.Channel.ChannelId " +
                $"WHERE dbo.Channel.ChannelId =@ChannelId ";

            var response = await _service.GetDataAsync<MessageViewModel>(query, new { ChannelId = channelId });
            return response.ToList();
        }

        public async Task SaveMessage(Message entity)
        {
            SqlTransaction? transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _service.SaveAsync(entity, _connection, transaction);
                transaction.Commit();
            }
            catch (Exception)
            {
                if (transaction != null) transaction.Rollback();
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task PutMessage(Guid id, Message message)
        {
            var query = $"UPDATE dbo.Message " +
                $"SET Content = '{message.Content}', UpdatedAt = CURRENT_TIMESTAMP " +
                $"WHERE dbo.Message.ID = @Id; ";

            var response = await _service.GetDataAsync(query, new { Id = id });
        }
        public async Task DeleteMessage(Guid id)
        {
            var query = $"DELETE FROM dbo.Message " +
                $"WHERE dbo.Message.ID = @Id;";

            var response = await _service.GetDataAsync(query, new { Id = id });
        }
        public async Task SaveChannel(Channel _channel)
        {
            SqlTransaction? transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _service.SaveAsync(_channel, _connection, transaction);
                //var details = _channel.Details.ToList();
                //await _service.SaveManyAsync<ChatRoomDetail>(details, _connection, transaction);

                transaction.Commit();
            }
            catch (Exception)
            {
                if (transaction != null) transaction.Rollback();
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<Tuple<Guid?, IEnumerable<string>>> GetChatChannelByUserIds(IEnumerable<string> userIds)
        {
            var query = $"SELECT dbo.Channel.ChannelId As channelId, Count(*) Over() As TotalUsers " +
                $"FROM dbo.Channel " +
                $"INNER JOIN dbo.Community ON dbo.Channel.ChannelCommunityId =dbo.Community.CommunityId " +
                $"INNER JOIN dbo.Users ON dbo.Community.CommunityOwnerId =dbo.Users.ID " +
                $"WHERE dbo.Users.ID In @UserIds" +
                $"" +
                $"SELECT dbo.Users.FirstName + ' ' + dbo.Users.LastName As Name" +
                $"FROM dbo.Users " +
                $"WHERE dbo.Users.ID IN @UserIds";

            using SqlConnection uow = _service.Connection;
            await uow.OpenAsync();
            var records = await uow.QueryMultipleAsync(query, new { UserIds = userIds });

            var rec = await records.ReadFirstOrDefaultAsync();
            Guid? channelId = null;

            if (rec != null)
            {
                var totalUsers = rec.TotalUsers;
                if (totalUsers == userIds.Count()) channelId = (Guid?)rec.channelId;
            }

            var userNames = await records.ReadAsync<string>();

            return new Tuple<Guid?, IEnumerable<string>>(channelId, userNames);
        }
        public async Task<List<ChatCardModel>> GetChatListByID(Guid userId)
        {
            var query = $"SELECT dbo.Message.ReceiverID,dbo.Message.Content,dbo.Message.SenderID " +
                $"FROM dbo.ChatList INNER JOIN dbo.Message ON dbo.ChatList.MessageID =dbo.Message.ID " +
                $"WHERE dbo.ChatList.UserID =@UserId";

            var response = await _service.GetDataAsync<ChatCardModel>(query, new { UserId = userId });
            return response.ToList();
        }
    }
}