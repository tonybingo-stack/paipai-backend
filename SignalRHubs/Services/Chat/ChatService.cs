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

        public ChatService(IDapperService<Message> service)
        {
            _service = service;
        }

        public async Task<IEnumerable<ChannelViewModel>> GetChatChannels(string username)
        {
            var query = $"SELECT dbo.Channel.ChannelId,dbo.Channel.ChannelName,dbo.Channel.ChannelDescription,dbo.Channel.ChannelCommunityId,dbo.Channel.CreatedAt,dbo.Channel.UpdatedAt " +
                $"FROM dbo.Channel INNER JOIN dbo.Community ON dbo.Channel.ChannelCommunityId =dbo.Community.ID " +
                $"INNER JOIN dbo.Users ON dbo.Community.CommunityOwnerName =dbo.Users.UserName " +
                $"WHERE dbo.Users.UserName =@Username";
            return await _service.GetDataAsync<ChannelViewModel>(query, new {Username=username});
        }

        public async Task<Message> GetMessage(Guid? Id)
        {
            if(Id == null) return null;

            var query = $"SELECT * " +
                $"FROM dbo.Message " +
                $"WHERE dbo.Message.ID =@Id";

            var response = await _service.GetDataAsync<Message>(query, new { Id = Id });
            if (response.Count==0) return null;
            else return response[0];
        }

        public async Task<IEnumerable<ChannelMessageViewModel>> GetMessageByChannelId(Guid channelId, int offset)
        {
            var query = $"SELECT A.SenderUserName,A.Content,A.FilePath, A.FileType, A.FilePreviewW, A.FilePreviewH, A.CreatedAt,A.RepliedTo,B.SenderUserName AS RepliedUserName," +
                $"C.Avatar AS RepliedUserAvatar,B.Content AS RepliedContent,B.CreatedAt AS RepliedMsgCreatedAt," +
                $"B.UpdatedAt AS RepliedMsgUpdatedAt " +
                $"FROM dbo.ChannelMessage AS A LEFT JOIN dbo.ChannelMessage AS B ON A.RepliedTo =B.ID LEFT JOIN dbo.Users AS C ON B.SenderUserName =C.UserName " +
                $"WHERE A.ChannelId=@id AND A.isDeleted=0" +
                $"ORDER BY A.CreatedAt ASC OFFSET @offset ROWS FETCH NEXT 10 ROWS ONLY;";

            var response = await _service.GetDataAsync<ChannelMessageViewModel>(query, new { id=channelId, offset=offset*10 });
            return response.ToList();
        }

        public async Task SaveMessage(List<Message> messages)
        {
            foreach (var entity in messages)
            {
                var obj = new
                {
                    Id = entity.Id,
                    senderUsername = entity.SenderUserName,
                    receiverUsername = entity.ReceiverUserName,
                    content = entity.Content,
                    filePath = entity.FilePath,
                    fileType = entity.FileType,
                    filePreviewW = entity.FilePreviewW,
                    filePreviewH = entity.FilePreviewH,
                    repliedTo = entity.RepliedTo
                };
                //  1 to 1 chat
                var query = $"INSERT INTO [dbo].[Message] " +
                    $"VALUES (@Id,@senderUsername, @receiverUsername,@content, @filePath,@fileType," +
                    $"@filePreviewW,@filePreviewH , CURRENT_TIMESTAMP, NULL, 0,@repliedTo )";

                await _service.GetDataAsync(query, obj);
            }
        }
        public async Task PutMessage(Message entity)
        {
            var query = $"UPDATE dbo.Message SET Content = @content,";
            if (entity.FilePath != null) query += $"FilePath=@filePath, ";
            if (entity.FileType != null) query += $"FileType=@fileType, ";
            if (entity.FilePreviewW != null) query += $"FilePreviewW=@fileW, ";
            if (entity.FilePreviewH != null) query += $"FilePreviewH=@fileH, ";
            query+= $"UpdatedAt = CURRENT_TIMESTAMP WHERE dbo.Message.ID = @id; ";

            var obj = new { 
                content = entity.Content,
                filePath = entity.FilePath,
                fileType = entity.FileType,
                fileW=entity.FilePreviewW,
                fileH=entity.FilePreviewH,
                id=entity.Id
            };
            await _service.GetDataAsync(query, obj);
        }
        public async Task DeleteMessage(Guid id)
        {
            var query = $"UPDATE dbo.Message " +
                $"SET UpdatedAt = CURRENT_TIMESTAMP, isDeleted=1 " +
                $"WHERE dbo.Message.ID = @Id; ";

            var response = await _service.GetDataAsync(query, new {Id = id});
        }

        public async Task<IEnumerable<ChatCardViewModel>> GetChatCards(string username)
        {
            var query = $"SELECT dbo.ChatCard.SenderUserName,dbo.ChatCard.ReceiverUserName,dbo.ChatCard.Content, dbo.ChatCard.FilePath, dbo.ChatCard.FileType ,dbo.ChatCard.isSend,dbo.Users.NickName,dbo.Users.Avatar " +
                $"FROM dbo.ChatCard INNER JOIN dbo.Users ON dbo.ChatCard.ReceiverUserName =dbo.Users.UserName " +
                $"WHERE dbo.ChatCard.SenderUserName= @user AND isDeleted = 0";

            var result = await _service.GetDataAsync<ChatCardViewModel>(query, new {user=username});
            return result.ToList();
        }
        public async Task<IEnumerable<ChatModel>> GetChatHistory(ChatHistoryBindingModel model, int offset)
        {
            var query = $"SELECT " +
                $"A.ID, A.SenderUserName,A.Content,A.FilePath, A.FileType, A.FilePreviewW, A.FilePreviewH, A.CreatedAt,A.RepliedTo," +
                $"B.SenderUserName AS RepliedUserName,B.Content AS RepliedContent,B.CreatedAt AS RepliedMsgCreatedAt " +
                $"FROM dbo.Message AS A LEFT JOIN dbo.Message AS B ON A.RepliedTo =B.ID " +
                $"WHERE (A.SenderUserName =@sender AND A.ReceiverUserName =@receiver) " +
                $"OR (A.SenderUserName =@receiver AND A.ReceiverUserName =@sender) " +
                $"ORDER BY A.CreatedAt DESC OFFSET @offset ROWS FETCH NEXT 10 ROWS ONLY;";

            var obj = new
            {
                sender = model.SenderUserName,
                receiver = model.ReceiverUserName,
                offset = offset * 10
            };
            var response = await _service.GetDataAsync<ChatModel>(query, obj);
            return response.ToList();
        }
        public async Task<string> DeleteAllChatCards(string username)
        {
            var query = $"UPDATE dbo.ChatCard SET isDeleted = 'TRUE'  WHERE dbo.ChatCard.SenderUserName =@user;";
            await _service.GetDataAsync(query, new {user=username});
            return "Deleted all chat cards";
        }
        public async Task<string> DeleteChatCardByID(Guid chatCardID)
        {
            var query = $"UPDATE dbo.ChatCard SET isDeleted = 'TRUE'  WHERE dbo.ChatCard.ID =@ID;";
            await _service.GetDataAsync(query, new { ID = chatCardID });
            return "Deleted chat card";
        }

        public async Task RefreshChatCard(string sender, string receiver, Message message)
        {
            var query =$"BEGIN " +
                $"DECLARE @i INT; " +
                $"SELECT @i = COUNT(* ) " +
                $"FROM dbo.ChatCard " +
                $"WHERE dbo.ChatCard.SenderUserName = @s AND dbo.ChatCard.ReceiverUserName = @r " +
                $"SELECT @i;" +
                $"IF @i > 0 " +
                $"BEGIN UPDATE dbo.ChatCard " +
                $"SET Content = @content, FilePath = @filePath, FileType = @fileType " +
                $"WHERE (SenderUserName=@s AND ReceiverUserName=@r) OR (SenderUserName=@r AND ReceiverUserName=@s);" +
                $"END " +
                $"ELSE " +
                $"BEGIN INSERT INTO dbo.ChatCard VALUES(NEWID(), @s, @r, @content,@filePath, @fileType, 1, 0); " +
                $"INSERT INTO dbo.ChatCard VALUES(NEWID(), @r, @s, @content,@filePath, @fileType, 0, 0); " +
                $"END " +
                $"END";
            var obj = new
            {
                s = sender,
                r = receiver,
                content = message.Content,
                filePath = message.FilePath,
                fileType = message.FileType
            };
            await _service.GetDataAsync(query, obj);
        }

        public async Task<ChannelMessage> GetChannelMessageById(Guid? id)
        {
            var query = $"SELECT * FROM dbo.ChannelMessage WHERE Id = @Id;";
            var res = await _service.GetDataAsync<ChannelMessage>(query, new {Id=id});
            if (res.Count == 0) return null;
            else return res[0];
        }

        public async Task UpdateChannelMessage(ChannelMessage m)
        {
            var query = $"UPDATE dbo.ChannelMessage " +
                $"SET Content=@content, FilePath=@filePath,FileType=@fileType, FilePreviewW=@fileW, FilePreviewH=@fileH, UpdatedAt=CURRENT_TIMESTAMP " +
                $"WHERE ID='{m.Id}';";
            var obj = new
            {
                content = m.Content,
                filePath = m.FilePath,
                fileType = m.FileType,
                fileW = m.FilePreviewW,
                fileH = m.FilePreviewH
            };
            await _service.GetDataAsync(query, obj);
        }

        public async Task DeleteChannelMessageById(Guid messageId)
        {
            var query = $"UPDATE dbo.ChannelMessage SET isDeleted = 1 WHERE Id=@ID;";
            await _service.GetDataAsync(query, new {ID=messageId});
        }

        public async Task SaveChannelMessage(ChannelMessage message)
        {
            // Channel Chat
            var query = $"INSERT INTO [dbo].[ChannelMessage] VALUES(@Id, @sname, @content, @filePath," +
                $" @fileType, @fileW, @fileH, @channelID, CURRENT_TIMESTAMP, NULL, 0,@repliedTo);";

            var obj = new
            {
                Id = message.Id,
                sname = message.SenderUserName, 
                content = message.Content,
                filePath = message.FilePath,
                fileType = message.FileType,
                fileW = message.FilePreviewW,
                fileH = message.FilePreviewH,
                channelID=message.ChannelId,
                repliedTo=message.RepliedTo
            };
            await _service.GetDataAsync(query, obj);
        }

        public async Task<string> CheckUserFriendShip(string userName, string receiverUserName)
        {
            var query = $"SELECT * FROM FriendList WHERE UserOne = @userone AND UserTwo = @usertwo";
            var response = await _service.GetDataAsync<FriendListModel>(query, new { userone = userName, usertwo = receiverUserName });
            if (response.Count == 0) return null;
            return response[0].Status;
        }
    }
}