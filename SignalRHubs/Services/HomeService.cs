using DbHelper.Interfaces.Services;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Lib;
using SignalRHubs.Models;
using System.ComponentModel.DataAnnotations;
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
                $"'{entity.Id}', " +
                $"'{entity.ChannelName}', ";

            if (entity.ChannelDescription != null) query += $"'{entity.ChannelDescription}', ";
            else query += $"NULL, ";

            query+= $"'{entity.ChannelOwnerName}', " +
                $"'{entity.ChannelCommunityId}', " +
                $"CURRENT_TIMESTAMP," +
                $"NULL" +
                $")";

            await _service.GetDataAsync(query);

            return entity.Id;
        }

        /// <summary>
        /// Create new community
        /// </summary>
        /// <param name="community"></param>
        /// <returns></returns>
        public async Task<Guid> CreateCommunity(Community entity)
        {
            var query = $"INSERT INTO Community VALUES( " +
                $"'{entity.Id}', " +
                $"'{entity.CommunityName}', ";
            if (entity.CommunityDescription == null) query += $"NULL, ";
            else query += $"'{entity.CommunityDescription}', ";

            query += $"'{entity.CommunityOwnerName}', ";

            if (entity.CommunityType == null) query += $"NULL, ";
            else query += $"'{entity.CommunityType}', ";

            query += $"CURRENT_TIMESTAMP, " + $"NULL,";

            if (entity.ForegroundImage == null) query += $"NULL, ";
            else query += $"'{entity.ForegroundImage}', ";

            if (entity.BackgroundImage == null) query += $"NULL, ";
            else query += $"'{entity.BackgroundImage}', ";

            query += $"0, 0, 0);";

            await _service.GetDataAsync(query);

            // Update CommunityMember Table
            query = $"INSERT INTO dbo.CommunityMember VALUES(NEWID(), '{entity.CommunityOwnerName}','{entity.Id}',0);";
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
            // Update referenced table participants, message, event
            var query = $"DELETE FROM dbo.Participants WHERE ChannelId='{id}'; " +
                $"UPDATE dbo.Message SET ChannelId = NULL,isDeleted = 1 WHERE ChannelId = '{id}';" +
                $"DELETE FROM dbo.Channel WHERE ChannelId = '{id}'; "; 

            await _service.GetDataAsync(query);
            return id;
        }  

        public async Task<Guid> DeleteCommunity(Guid id)
        {
            // Update referenced table channel, communitymember, Posts
            //var query = $"DELETE FROM dbo.CommunityMember WHERE CommunityID='{id}'";
            //await _service.GetDataAsync(query);

            //query = $"SELECT dbo.Channel.ChannelId,dbo.Channel.ChannelName,dbo.Channel.ChannelDescription,dbo.Channel.ChannelOwnerName,dbo.Channel.ChannelCommunityId,dbo.Channel.CreatedAt,dbo.Channel.UpdatedAt FROM dbo.Channel WHERE dbo.Channel.ChannelCommunityId='{id}';";
            //var response = await _service.GetDataAsync<Channel>(query);
            //foreach(var item in response)
            //{
            //    await DeleteChannel(item.ChannelId);
            //}

            //query = $"DELETE FROM dbo.Posts WHERE CommunityID='{id}';";
            //await _service.GetDataAsync(query);

            //query = $"UPDATE dbo.Event SET CommunityId=NULL, isDeleted=1 WHERE CommunityId='{id}'";
            //await _service.GetDataAsync(query);
            //// Update referenced table

            //query = $"DELETE FROM dbo.Community WHERE ID = '{id}';";
            var query = $"DECLARE @Counter INT, @i INT " +
                $"WITH x as (SELECT* FROM dbo.Channel WHERE dbo.Channel.ChannelCommunityId = '{id}') " +
                $"SELECT @Counter = COUNT(*) FROM x " +
                $"SET @i = 1 " +
                $"WHILE(@i <= @Counter) " +
                $"BEGIN " +
                $"DECLARE @Id uniqueidentifier " +
                $"SELECT TOP 1 @Id = ChannelId FROM(SELECT TOP(@i) * FROM dbo.Channel WHERE dbo.Channel.ChannelCommunityId = '{id}' ORDER BY ChannelId ASC) tmp ORDER BY ChannelId DESC " +
                $"DELETE FROM dbo.Participants WHERE ChannelId = @Id; " +
                $"UPDATE dbo.Message SET ChannelId = NULL,isDeleted = 1 WHERE ChannelId = @Id; " +
                $"DELETE FROM dbo.Channel WHERE ChannelId = @Id; " +
                $"PRINT 'The counter value is = ' + CONVERT(nvarchar(36), @Id) " +
                $"SET @i = @i + 1 " +
                $"END " +
                $"DELETE FROM dbo.CommunityMember WHERE CommunityID = '{id}'; " +
                $"DELETE FROM dbo.Posts WHERE CommunityID = '{id}'; " +
                $"UPDATE dbo.Event SET CommunityId = NULL, isDeleted = 1 WHERE CommunityId = '{id}'; " +
                $"DELETE FROM dbo.Community WHERE ID = '{id}'; ";

            await _service.GetDataAsync(query);

            return id;
        }

        public async Task<IEnumerable<ChannelViewModel>> GetAllChannels(Guid communityID)
        {
            var query = $"SELECT * FROM dbo.Channel WHERE ChannelCommunityId = '{communityID}';";
            var response = await _service.GetDataAsync<ChannelViewModel>(query);
            return response.ToList();
        }

        public async Task<Guid> UpdateChannel(ChannelUpdateModel model)
        {
            var query = $"UPDATE dbo.Channel SET ";
            if (model.ChannelName != null) query += $"ChannelName='{model.ChannelName}'";
            if (model.ChannelDescription != null) query += $",ChannelDescription = '{model.ChannelDescription}'";

            query += $", UpdatedAt = CURRENT_TIMESTAMP WHERE dbo.Channel.ChannelId = '{model.ChannelId}';";

            var response = await _service.GetDataAsync(query);
            return model.ChannelId;
        }

        public async Task<Guid> UpdateCommunity(Community entity)
        {
            bool flag = false;
            var query = $"UPDATE dbo.Community SET ";
            if (entity.CommunityName != null)
            {
                query += $"CommunityName='{entity.CommunityName}'";
                flag = true;
            }
            if (entity.CommunityDescription != null)
            {
                if (flag) query += $", ";
                query += $"CommunityDescription = '{entity.CommunityDescription}'";
                flag = true;
            }
            if (entity.CommunityType != null)
            {
                if (flag) query += $", ";
                query += $" CommunityType = '{entity.CommunityType}'";
                flag = true;
            }
            if (flag) query += $", ";
            query += $" UpdatedAt = CURRENT_TIMESTAMP";

            if (entity.ForegroundImage != null) query += $", ForegroundImage = '{entity.ForegroundImage}'";
            if (entity.BackgroundImage != null) query += $", BackgroundImage = '{entity.BackgroundImage}'";
            if (entity.NumberOfUsers != null) query += $", NumberOfUsers = '{entity.NumberOfUsers}'";
            if (entity.NumberOfPosts != null) query += $", NumberOfPosts = '{entity.NumberOfPosts}'";
            if (entity.NumberOfActiveUsers != null) query += $", NumberOfActiveUsers = '{entity.NumberOfActiveUsers}'";
            query += $" WHERE dbo.Community.ID = '{entity.Id}'; ";

            Console.WriteLine("here:"+query);

            await _service.GetDataAsync(query);  

            return entity.Id;
        }

        public async Task<Guid> CreatePost(Post entity)
        {
            var query = $"INSERT INTO dbo.Posts OUTPUT Inserted.ID Values(NEWID(), '{entity.UserName}', '{entity.CommunityID}', '{entity.Title}', ";
            if (entity.Contents != null) query += $"'{entity.Contents}',";
            else query += $"NULL, ";

            if (entity.Price != null) query += $"'{entity.Price}',";
            else query += $"NULL, ";

            if (entity.Category != null) query += $"'{entity.Category}',";
            else query += $"NULL, ";

            query += $"'{entity.CreatedAt}', NULL, 'FALSE')";

            var response = await _service.GetDataAsync<Post>(query);

            return response[0].Id;
        }

        public async Task<IEnumerable<PostViewModel>> GetPosts(Guid communityID, string username)
        {
            var query = $"SELECT dbo.Posts.ID,dbo.Posts.Title,dbo.Posts.Contents,dbo.Posts.Price,dbo.Posts.Category,dbo.Posts.CreatedAt,dbo.Posts.UpdatedAt FROM dbo.Posts WHERE dbo.Posts.UserName ='{username}' AND dbo.Posts.CommunityID ='{communityID}' AND dbo.Posts.isDeleted =0 ORDER BY dbo.Posts.CreatedAt DESC";
            var response = await _service.GetDataAsync<PostViewModel>(query);

            return response.ToList();
        }

        public async Task<Guid> UpdatePost(PostUpdateModel model)
        {
            bool flag=false;
            var query = $"UPDATE dbo.Posts SET ";
            if (model.Title != null)
            {
                query += $"Title='{model.Title}'";
                flag = true;
            }
            if (model.Contents != null)
            {
                if (flag) query += $", Contents='{model.Contents}'";
                else query += $"Contents='{model.Contents}'";
                flag = true;
            }
            if (model.Price != null)
            {
                if(flag) query += $", Price='{model.Price}'";
                else query += $" Price='{model.Price}'";
                flag = true;
            }
            if (model.Category != null)
            {
                if(flag) query += $", Category='{model.Category}'";
                else query += $" Category='{model.Category}'";
                flag=true;
            }
            query += $", UpdatedAt=CURRENT_TIMESTAMP WHERE ID='{model.Id}'";
            await _service.GetDataAsync(query);

            return model.Id;
        }

        public async Task<Guid> DeletePost(Guid postId)
        {
            var query = $"UPDATE dbo.Posts SET isDeleted=1 OUTPUT Deleted.CommunityID as ID WHERE ID='{postId}';";
            var response = await _service.GetDataAsync(query);
            GlobalModule.NumberOfPosts[response[0].Id.ToString()] = (int)GlobalModule.NumberOfPosts[response[0].Id.ToString()] - 1;
            return postId;
        }

        public async Task<IEnumerable<CommunityViewModel>> GetJoinedCommunity(string username)
        {
            var query = $"SELECT dbo.Community.ID,dbo.Community.CommunityName,dbo.Community.CommunityDescription,dbo.Community.CommunityOwnerName,dbo.Community.CommunityType,dbo.Community.CreatedAt,dbo.Community.UpdatedAt,dbo.Community.ForegroundImage,dbo.Community.BackgroundImage,dbo.Community.NumberOfUsers,dbo.Community.NumberOfPosts,dbo.Community.NumberOfActiveUsers FROM dbo.Community INNER JOIN dbo.CommunityMember ON dbo.CommunityMember.CommunityID =dbo.Community.ID WHERE dbo.CommunityMember.UserName ='{username}'";
            var response = await _service.GetDataAsync<CommunityViewModel>(query);
            return response.ToList();
        }

        public async Task<string> JoinCommunity(string username, Guid communityId)
        {
            var query = $"INSERT INTO dbo.CommunityMember VALUES(NEWID(), '{username}','{communityId}',2);";
            await _service.GetDataAsync(query);
            return $"'{username}' Joined Community.";
        }

        public async Task<string> ExitCommunity(string username, Guid communityId)
        {
            var query = $"DELETE FROM dbo.CommunityMember WHERE UserName='{username} AND CommunityID='{communityId}';";
            await _service.GetDataAsync(query);
            return $"'{username}' exited from Community.";
        }

        public async Task<CommunityMember> GetUserRole(string username, Guid communityId)
        {
            var query = $"SELECT * FROM dbo.CommunityMember WHERE UserName='{username}' AND CommunityID='{communityId}';";
            var response = await _service.GetDataAsync<CommunityMember>(query);

            CommunityMember m = new CommunityMember();
            m.UserRole = 4;
            if (response.Count == 0) return m;
            return response[0];
        }

        public async Task<Channel> GetChannelById(Guid channelId)
        {
            var query = $"SELECT * FROM dbo.Channel WHERE ChannelId='{channelId}';";
            var response = await _service.GetDataAsync<Channel>(query);

            if (response.Count == 0) return null;
            return response[0];
        }

        public async Task<Community> GetCommunityById(Guid id)
        {
            var query = $"SELECT*FROM dbo.Community WHERE ID ='{id}'";
            var response = await _service.GetDataAsync(query);
            if (response.Count == 0) return null;
            return response[0];
        }

        public async Task<Guid> CreateEvent(Event e)
        {
            var query = $"INSERT INTO dbo.Event OUTPUT Inserted.ID VALUES(NEWID(), '{e.Title}', '{e.HostName}', ";
            
            if (e.Description != null) query += $"'{e.Description}', ";
            else query += $"NULL, ";

            if (e.Time != null) query += $"'{e.Time}',";
            else query += $"NULL, ";

            if (e.Access != null) query += $"'{e.Access}', ";
            else query += $"NULL, ";

            if (e.Image != null) query += $"'{e.Image}', ";
            else query += $"NULL, ";

            if (e.Limit != null) query += $"{e.Limit}, ";
            else query += $"NULL, ";

            query += $"'{e.CommunityId}', CURRENT_TIMESTAMP, NULL, 0);";

            var response = await _service.GetDataAsync<Event>(query);
            if (response.Count == 0) return Guid.Empty;

            return response[0].Id;
        }

        public async Task<Event> GetEventByID(Guid id)
        {
            var query = $"SELECT * FROM dbo.Event WHERE Id='{id}'";
            var response = await _service.GetDataAsync<Event>(query);
            if (response.Count == 0) return null;
            return response[0];
        }

        public async Task<Guid> UpdateEvent(Event e)
        {
            var query = $"UPDATE dbo.Event " +
                $"SET Title='{e.Title}', Description='{e.Description}', Time='{e.Time}', Access='{e.Access}',Image='{e.Image}', Limit='{e.Limit}', UpdatedAt=CURRENT_TIMESTAMP " +
                $"WHERE ID='{e.Id}'";

            var response =await _service.GetDataAsync<Event>(query);
            return e.Id;
        }

        public async Task<Guid> DeleteEvent(Guid id)
        {
            var query = $"UPDATE dbo.Event SET isDeleted=1 WHERE ID='{id}'";
            await _service.GetDataAsync(query);

            return id;
        }

        public async Task<IEnumerable<EventViewModel>> GetAllEvent(Guid communityId)
        {
            var query = $"SELECT * FROM dbo.Event Where CommunityId='{communityId}' AND isDeleted=0;";
            var response = await _service.GetDataAsync<EventViewModel>(query);

            return response.ToList();
        }

        public async Task<Community> GetCommunityByChannelId(Guid channelId)
        {
            var query = $"SELECT*FROM dbo.Community INNER JOIN dbo.Channel ON dbo.Channel.ChannelCommunityId =dbo.Community.ID WHERE dbo.Channel.ChannelId='{channelId}';";
            var response = await _service.GetDataAsync<Community>(query);
            if (response.Count == 0) return null;
            return response[0];
        }
    }
}
