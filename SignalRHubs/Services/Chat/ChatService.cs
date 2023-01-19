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
            var query = $"SELECT dbo.Channel.ChannelId,dbo.Channel.ChannelName,dbo.Channel.ChannelDescription,dbo.Channel.ChannelCommunityId,dbo.Channel.CreatedAt,dbo.Channel.UpdatedAt FROM dbo.Channel INNER JOIN dbo.Community ON dbo.Channel.ChannelCommunityId =dbo.Community.ID INNER JOIN dbo.Users ON dbo.Community.CommunityOwnerName =dbo.Users.UserName WHERE dbo.Users.UserName =N'{username}'";
            return await _service.GetDataAsync<ChannelViewModel>(query);
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

        public async Task<IEnumerable<ChannelMessageViewModel>> GetMessageByChannelId(Guid channelId, int offset)
        {
            var query = $"SELECT A.SenderUserName,A.Content,A.CreatedAt,A.RepiedTo,B.SenderUserName AS RepliedUserName," +
                $"C.Avatar AS RepliedUserAvatar,B.Content AS RepliedContent,B.CreatedAt AS RepliedMsgCreatedAt," +
                $"B.UpdatedAt AS RepliedMsgUpdatedAt " +
                $"FROM dbo.ChannelMessage AS A LEFT JOIN dbo.ChannelMessage AS B ON A.RepiedTo =B.ID LEFT JOIN dbo.Users AS C ON B.SenderUserName =C.UserName " +
                $"WHERE A.ChannelId='{channelId}' AND A.isDeleted=0" +
                $"ORDER BY A.CreatedAt ASC OFFSET {offset*10} ROWS FETCH NEXT 10 ROWS ONLY;";

            var response = await _service.GetDataAsync<ChannelMessageViewModel>(query);
            return response.ToList();
        }

        public async Task SaveMessage(Message entity)
        {
            var query="";
            if (entity.ChannelId == null)
            {
                //  1 to 1 chat
                query = $"INSERT INTO [dbo].[Message] " +
                    $"VALUES (NEWID(),N'{entity.SenderUserName}',N'{entity.ReceiverUserName}',N'{entity.Content}', NULL";
                if (entity.FilePath != null) query += $", N'{entity.FilePath}'";
                else query += $", NULL";
                query += $", CURRENT_TIMESTAMP, NULL, 'FALSE', ";
                if (entity.RepliedTo != null) query += $"'{entity.RepliedTo}');";
                else query += $"NULL); ";

                query += $"BEGIN DECLARE @i INT; " +
                    $"SELECT @i = COUNT(*) FROM dbo.ChatCard " +
                    $"WHERE dbo.ChatCard.SenderUserName = N'{entity.SenderUserName}' AND dbo.ChatCard.ReceiverUserName = N'{entity.ReceiverUserName}'" +
                    $"SELECT @i; IF @i> 0 BEGIN UPDATE dbo.ChatCard " +
                    $"SET Content = N'{entity.Content}',isSend = 1,isDeleted = 0 WHERE SenderUserName = N'{entity.SenderUserName}' AND ReceiverUserName = N'{entity.ReceiverUserName}'; " +
                    $"UPDATE dbo.ChatCard " +
                    $"SET Content = N'{entity.Content}',isSend = 0,isDeleted = 0 WHERE SenderUserName = N'{entity.ReceiverUserName}' AND ReceiverUserName = N'{entity.SenderUserName}'; " +
                    $"END ELSE BEGIN " +
                    $"INSERT INTO dbo.ChatCard VALUES(NEWID(), N'{entity.SenderUserName}',N'{entity.ReceiverUserName}',N'{entity.Content}',1,0); " +
                    $"INSERT INTO dbo.ChatCard VALUES(NEWID(), N'{entity.ReceiverUserName}',N'{entity.SenderUserName}',N'{entity.Content}',0,0); END END; ";
            }
            else
            {
                // Channel Chat
                query = $"INSERT INTO [dbo].[ChannelMessage] VALUES(NEWID(), '{entity.SenderUserName}', '{entity.Content}'";
                if (entity.FilePath != null) query += $", N'{entity.FilePath}'";
                else query += $", NULL";
                query += $", '{entity.ChannelId}', CURRENT_TIMESTAMP, NULL, 0, ";
                if (entity.RepliedTo != null) query += $"'{entity.RepliedTo}');";
                else query += $"NULL);";
            }
            await _service.GetDataAsync(query);
        }
        public async Task PutMessage(Message entity)
        {
            var query = $"UPDATE dbo.Message " +
                $"SET Content = N'{entity.Content}', UpdatedAt = CURRENT_TIMESTAMP " +
                $"WHERE dbo.Message.ID = '{entity.Id}'; ";
            await _service.GetDataAsync(query);
        }
        public async Task DeleteMessage(Guid id)
        {
            var query = $"UPDATE dbo.Message " +
                $"SET UpdatedAt = CURRENT_TIMESTAMP, isDeleted='TRUE' " +
                $"WHERE dbo.Message.ID = @Id; ";

            var response = await _service.GetDataAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<ChatCardModel>> GetChatCards(string username)
        {
            var query = $"SELECT dbo.ChatCard.SenderUserName,dbo.ChatCard.ReceiverUserName,dbo.ChatCard.Content,dbo.ChatCard.isSend,dbo.Users.NickName,dbo.Users.Avatar FROM dbo.ChatCard INNER JOIN dbo.Users ON dbo.ChatCard.ReceiverUserName =dbo.Users.UserName WHERE dbo.ChatCard.SenderUserName= N'{username}' AND isDeleted = 0";
            var result = await _service.GetDataAsync<ChatCardModel>(query);
            return result.ToList();
        }
        public async Task<IEnumerable<ChatModel>> GetChatHistory(ChatHistoryBindingModel model, int offset)
        {
            var query = $"SELECT " +
                $"A.SenderUserName,A.Content,A.CreatedAt,A.RepliedTo," +
                $"B.SenderUserName AS RepliedUserName,B.Content AS RepliedContent,B.CreatedAt AS RepliedMsgCreatedAt " +
                $"FROM dbo.Message AS A LEFT JOIN dbo.Message AS B ON A.RepliedTo =B.ID " +
                $"WHERE (A.SenderUserName =N'{model.SenderUserName}' AND A.ReceiverUserName =N'{model.ReceiverUserName}') OR (A.SenderUserName =N'{model.ReceiverUserName}' AND A.ReceiverUserName =N'{model.SenderUserName}') " +
                $"ORDER BY A.CreatedAt DESC OFFSET {offset*10} ROWS FETCH NEXT 10 ROWS ONLY;";

            var response = await _service.GetDataAsync<ChatModel>(query);
            return response.ToList();
        }
        public async Task<string> DeleteAllChatCards(string username)
        {
            var query = $"UPDATE dbo.ChatCard SET isDeleted = 'TRUE'  WHERE dbo.ChatCard.SenderUserName =N'{username}';";
            await _service.GetDataAsync(query);
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
                $"WHERE dbo.Message.SenderUserName =N'{sender}' AND " +
                $"dbo.Message.ReceiverUserName =N'{receiver}' AND " +
                $"dbo.Message.isDeleted =0 " +
                $"ORDER BY dbo.Message.CreatedAt DESC";

            var response = await _service.GetDataAsync(query);
            if (response == null) return;
            query = $"UPDATE dbo.ChatCard SET Content=N'{response[0].Content.Replace("'","''")}' WHERE (SenderUserName=N'{sender}' AND ReceiverUserName=N'{receiver}') OR (SenderUserName=N'{receiver}' AND ReceiverUserName=N'{sender}')";

            await _service.GetDataAsync(query);
        }

        public async Task<ChannelMessage> GetChannelMessageById(Guid id)
        {
            var query = $"SELECT * FROM dbo.ChannelMessage WHERE Id = '{id}';";
            var res = await _service.GetDataAsync<ChannelMessage>(query);
            if (res.Count == 0) return null;
            else return res[0];
        }

        public async Task UpdateChannelMessage(ChannelMessage m)
        {
            var query = $"UPDATE dbo.ChannelMessage SET Content='{m.Content}', FilePath='{m.FilePath}', UpdatedAt=CURRENT_TIMESTAMP WHERE ID='{m.Id}';";
            await _service.GetDataAsync(query);
        }

        public async Task DeleteChannelMessageById(Guid messageId)
        {
            var query = $"UPDATE dbo.ChannelMessage SET isDeleted = 1 WHERE Id='{messageId}';";
            await _service.GetDataAsync(query);
        }
    }
}