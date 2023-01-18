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

        public async Task<IEnumerable<ChannelViewModel>> GetChatChannels(string username)
        {
            var query = $"SELECT dbo.Channel.ChannelId,dbo.Channel.ChannelName,dbo.Channel.ChannelDescription,dbo.Channel.ChannelCommunityId,dbo.Channel.CreatedAt,dbo.Channel.UpdatedAt FROM dbo.Channel INNER JOIN dbo.Community ON dbo.Channel.ChannelCommunityId =dbo.Community.ID INNER JOIN dbo.Users ON dbo.Community.CommunityOwnerName =dbo.Users.UserName WHERE dbo.Users.UserName =@Username";
            return await _service.GetDataAsync<ChannelViewModel>(query, new {Username = username });
        }

        public async Task<MessageViewModel> GetMessage(Guid Id)
        {
            var query = $"SELECT " +
                $"dbo.Message.ID, " +
                $"dbo.Message.SenderUserName," +
                $"dbo.Message.ReceiverUserName," +
                $"dbo.Message.Content," +
                $"dbo.Message.ChannelID," +
                $"dbo.Message.FilePath, " +
                $"dbo.Message.CreatedAt, " +
                $"dbo.Message.UpdatedAt " +
                $"dbo.Message.RepliedTo" +
                $"FROM dbo.Message " +
                $"WHERE dbo.Message.ID =@Id";

            var response = await _service.GetDataAsync<MessageViewModel>(query, new { Id = Id });
            if (response == null) return null;
            else return response[0];
        }

        public async Task<IEnumerable<ChannelChatModel>> GetMessageByChannelId(Guid channelId)
        {
            var query = $"SELECT " +
                $"A.SenderUserName,A.ReceiverUserName,A.Content,A.CreatedAt,A.RepliedTo," +
                $"B.SenderUserName AS RepliedUserName," +
                $"C.Avatar AS RepliedUserAvatar,B.Content AS RepliedContent,B.CreatedAt AS RepliedMsgCreatedAt " +
                $"FROM dbo.Message AS A LEFT JOIN dbo.Message AS B ON A.RepliedTo =B.ID LEFT JOIN dbo.Users AS C ON B.SenderUserName =C.UserName " +
                $"WHERE A.ChannelID='{channelId}' " +
                $"ORDER BY A.CreatedAt DESC OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;";

            var response = await _service.GetDataAsync<ChannelChatModel>(query);
            return response.ToList();
        }

        public async Task SaveMessage(Message entity)
        {
            var query = $"INSERT INTO [dbo].[Message] " +
                $"VALUES (NEWID(),'{entity.SenderUserName}','{entity.ReceiverUserName}','{entity.Content}'";
            if (entity.ChannelId != null) query += $", '{entity.ChannelId}'";
            else query += $", NULL";
            if (entity.FilePath != null) query += $", '{entity.FilePath}'";
            else query += $", NULL";
            query += $", CURRENT_TIMESTAMP, NULL, 'FALSE', ";
            if (entity.RepliedTo != null) query += $"'{entity.RepliedTo}');";
            else query += $"NULL);";

            await _service.GetDataAsync(query);
        }
        public async Task PutMessage(Message message)
        {
            var query = $"UPDATE dbo.Message " +
                $"SET Content = '{message.Content}', UpdatedAt = CURRENT_TIMESTAMP " +
                $"WHERE dbo.Message.ID = @Id; ";

            await _service.GetDataAsync(query, new { Id = message.Id });
        }
        public async Task DeleteMessage(Guid id)
        {
            var query = $"UPDATE dbo.Message " +
                $"SET UpdatedAt = CURRENT_TIMESTAMP, isDeleted='TRUE' " +
                $"WHERE dbo.Message.ID = @Id; ";

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

        public async Task CreateOrUpdateChatCards(ChatCardModel model)
        {
            var query = $"BEGIN " +
                $"DECLARE " +
                $"@i INT; " +
                $"SELECT " +
                $"@i = COUNT(* ) " +
                $"FROM " +
                $"dbo.ChatCard " +
                $"WHERE " +
                $"dbo.ChatCard.SenderUserName = '{model.SenderUserName}' " +
                $"AND dbo.ChatCard.ReceiverUserName = '{model.ReceiverUserName}' " +
                $"SELECT @i; " +
                $"IF " +
                $"@i > 0 BEGIN " +
                $"UPDATE dbo.ChatCard SET Content = '{model.Content}', isSend = '{model.isSend}', isDeleted = '{model.isDeleted}' WHERE SenderUserName = '{model.SenderUserName}' AND ReceiverUserName = '{model.ReceiverUserName}';" +
                $"END ELSE BEGIN " +
                $"INSERT INTO dbo.ChatCard VALUES(NEWID(), '{model.SenderUserName}', '{model.ReceiverUserName}', '{model.Content}', '{model.isSend}', '{model.isDeleted}');" +
                $"END " +
                $"END";

            await _service.GetDataAsync(query);
         
        }
        public async Task<IEnumerable<ChatCardModel>> GetChatCards(string username)
        {
            var query = $"SELECT dbo.ChatCard.SenderUserName,dbo.ChatCard.ReceiverUserName,dbo.ChatCard.Content,dbo.ChatCard.isSend,dbo.ChatCard.isDeleted,dbo.Users.NickName,dbo.Users.Avatar FROM dbo.ChatCard INNER JOIN dbo.Users ON dbo.ChatCard.ReceiverUserName =dbo.Users.UserName WHERE dbo.ChatCard.SenderUserName= '{username}' AND isDeleted = 0";
            var result = await _service.GetDataAsync<ChatCardModel>(query);
            return result.ToList();
        }
        public async Task<IEnumerable<ChatModel>> GetChatHistory(ChatHistoryBindingModel model, int offset)
        {
            var query = $"SELECT " +
                $"A.SenderUserName,A.Content,A.CreatedAt,A.RepliedTo," +
                $"B.SenderUserName AS RepliedUserName,B.Content AS RepliedContent,B.CreatedAt AS RepliedMsgCreatedAt " +
                $"FROM dbo.Message AS A LEFT JOIN dbo.Message AS B ON A.RepliedTo =B.ID " +
                $"WHERE (A.SenderUserName ='user01' AND A.ReceiverUserName ='user02') OR (A.SenderUserName ='user02' AND A.ReceiverUserName ='user01') " +
                $"ORDER BY A.CreatedAt DESC OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;";

            var response = await _service.GetDataAsync<ChatModel>(query);
            return response.ToList();
        }
        public async Task<string> DeleteAllChatCards(string username)
        {
            var query = $"UPDATE dbo.ChatCard SET isDeleted = 'TRUE'  WHERE dbo.ChatCard.SenderUserName =@Username;";
            await _service.GetDataAsync(query, new { Username = username });
            return "Deleted all chat cards";
        }
        public async Task<string> DeleteChatCardByID(Guid chatCardID)
        {
            var query = $"UPDATE dbo.ChatCard SET isDeleted = 'TRUE'  WHERE dbo.ChatCard.ID =@ID;";
            await _service.GetDataAsync(query, new { ID = chatCardID });
            return "Deleted chat card";
        }

        public async Task RefreshChatCard(string sender, string receiver)
        {
            var query = $"SELECT TOP 1 * FROM dbo.Message " +
                $"WHERE dbo.Message.SenderUserName ='{sender}' AND " +
                $"dbo.Message.ReceiverUserName ='{receiver}' AND " +
                $"dbo.Message.isDeleted =0 " +
                $"ORDER BY dbo.Message.CreatedAt DESC";

            var response = await _service.GetDataAsync(query);
            if (response == null) return;
            query = $"UPDATE dbo.ChatCard SET Content='{response[0].Content.Replace("'","''")}' WHERE (SenderUserName='{sender}' AND ReceiverUserName='{receiver}') OR (SenderUserName='{receiver}' AND ReceiverUserName='{sender}')";

            await _service.GetDataAsync(query);
        }
    }
}