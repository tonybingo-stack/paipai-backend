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
            var query = $"INSERT INTO Channel OUTPUT Inserted.ChannelId as Id VALUES(" +
                $"NEWID(), " +
                $"N'{entity.ChannelName}', ";

            if (entity.ChannelDescription != null) query += $"N'{entity.ChannelDescription}', ";
            else query += $"NULL, ";

            query+= $"N'{entity.ChannelOwnerName}', " +
                $"'{entity.ChannelCommunityId}', " +
                $"CURRENT_TIMESTAMP," +
                $"NULL" +
                $")";

            var res = await _service.GetDataAsync(query);
            //update channelmember
            query = $"INSERT INTO [dbo].[ChannelMember] VALUES (NEWID(), '{entity.ChannelOwnerName}','{res[0].Id}',1);";
            await _service.GetDataAsync(query);

            return res[0].Id;
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
                $"N'{entity.CommunityName}', ";

            if (entity.CommunityDescription == null) query += $"NULL, ";
            else query += $"N'{entity.CommunityDescription}', ";

            query += $"N'{entity.CommunityOwnerName}', ";

            if (entity.CommunityType == null) query += $"NULL, ";
            else query += $"N'{entity.CommunityType}', ";

            query += $"CURRENT_TIMESTAMP, " + $"NULL,";

            if (entity.ForegroundImage == null) query += $"NULL, ";
            else query += $"'{entity.ForegroundImage}', ";

            if (entity.BackgroundImage == null) query += $"NULL, ";
            else query += $"'{entity.BackgroundImage}', ";

            query += $"0, 0, 0);";

            await _service.GetDataAsync(query);

            // Update CommunityMember Table
            query = $"INSERT INTO dbo.CommunityMember VALUES(NEWID(), N'{entity.CommunityOwnerName}','{entity.Id}',0);";
            await _service.GetDataAsync(query);

            return entity.Id;
        }

        public async Task<string> UpdateCommunityAvatar(Guid id, string url)
        {
            var query = $"UPDATE [dbo].[Community] SET ForegroundImage = N'{url}' WHERE ID = '{id}'";
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
            // Update referenced table ChannelMember, message, event
            var query = $"DELETE FROM dbo.ChannelMember WHERE ChannelId='{id}'; " +
                $"DELETE dbo.ChannelMessage WHERE ChannelId = '{id}';" +
                $"DELETE FROM dbo.Channel WHERE ChannelId = '{id}'; "; 

            await _service.GetDataAsync(query);
            return id;
        }  

        public async Task<Guid> DeleteCommunity(Guid id)
        {
            var query = $"DECLARE @Counter INT, @i INT " +
                $"WITH x as (SELECT* FROM dbo.Channel WHERE dbo.Channel.ChannelCommunityId = '{id}') " +
                $"SELECT @Counter = COUNT(*) FROM x " +
                $"SET @i = 1 " +
                $"WHILE(@i <= @Counter) " +
                $"BEGIN " +
                $"DECLARE @Id uniqueidentifier " +
                $"SELECT TOP 1 @Id = ChannelId FROM(SELECT TOP(@i) * FROM dbo.Channel WHERE dbo.Channel.ChannelCommunityId = '{id}' ORDER BY ChannelId ASC) tmp ORDER BY ChannelId DESC " +
                $"DELETE FROM dbo.ChannelMember WHERE ChannelId = @Id; " +
                $"DELETE dbo.ChannelMessage WHERE ChannelId = @Id;" +
                $"DELETE FROM dbo.Channel WHERE ChannelId = @Id; " +
                $"PRINT 'The counter value is = ' + CONVERT(nvarchar(36), @Id) " +
                $"SET @i = @i + 1 " +
                $"END " +
                $"DELETE FROM dbo.CommunityMember WHERE CommunityID = '{id}'; " +
                $"DELETE FROM dbo.PostWithCommunity WHERE CommunityId = '{id}'; " +
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
            if (model.ChannelName != null) query += $"ChannelName=N'{model.ChannelName}'";
            if (model.ChannelDescription != null) query += $",ChannelDescription = N'{model.ChannelDescription}'";

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
                query += $"CommunityName=N'{entity.CommunityName}'";
                flag = true;
            }
            if (entity.CommunityDescription != null)
            {
                if (flag) query += $", ";
                query += $"CommunityDescription = N'{entity.CommunityDescription}'";
                flag = true;
            }
            if (entity.CommunityType != null)
            {
                if (flag) query += $", ";
                query += $" CommunityType = N'{entity.CommunityType}'";
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

            await _service.GetDataAsync(query);  

            return entity.Id;
        }

        public async Task<Guid> CreatePost(Post entity, List<Guid>? CommunityIds)
        {
            var query = $"INSERT INTO dbo.Posts OUTPUT Inserted.ID Values(NEWID(), N'{entity.UserName}', N'{entity.Title}', ";
            if (entity.Contents != null) query += $"N'{entity.Contents}',";
            else query += $"NULL, ";

            if (entity.Price != null) query += $"N'{entity.Price}',";
            else query += $"NULL, ";

            if (entity.Category != null) query += $"N'{entity.Category}',";
            else query += $"NULL, ";

            query += $"CURRENT_TIMESTAMP, NULL, 'FALSE')";
            var response = await _service.GetDataAsync<Post>(query);
            query = "";
            if (CommunityIds != null)
            {
                for(int i = 0; i < CommunityIds.Count; i++)
                {
                    query += $"INSERT INTO dbo.PostWithCommunity VALUES(NEWID(), '{response[0].Id}', '{CommunityIds[i]}');";
                }
            }
            await _service.GetDataAsync(query);

            return response[0].Id;
        }

        public async Task<IEnumerable<PostViewModel>> GetPosts(string username)
        {
            List<PostViewModel> result = new List<PostViewModel>();
            var query = $"SELECT * " +
                $"FROM dbo.Posts WHERE dbo.Posts.UserName =N'{username}' AND dbo.Posts.isDeleted =0 " +
                $"ORDER BY dbo.Posts.CreatedAt DESC";
            var response = await _service.GetDataAsync<PostViewModel>(query);
            if (response.Count == 0) return result;

            result = response.ToList();
            for(int i=0; i<result.Count; i++)
            {
                query = $"SELECT Users.* " +
                    $"FROM [dbo].[Posts] " +
                    $"INNER JOIN PostLikeUser ON Posts.ID=PostLikeUser.PostId " +
                    $"INNER JOIN Users ON PostLikeUser.UserName=Users.UserName " +
                    $"WHERE Posts.ID='{result[i].ID}'";
                var users = await _service.GetDataAsync<UserViewModel>(query);
                result[i].UsersLikePost = users.ToList();
                query = $"SELECT Community.*" +
                    $"FROM [dbo].[Posts] " +
                    $"INNER JOIN PostWithCommunity ON Posts.ID=PostWithCommunity.PostId " +
                    $"INNER JOIN Community ON PostWithCommunity.CommunityId=Community.ID " +
                    $"WHERE Posts.ID='{result[i].ID}'";
                var community = await _service.GetDataAsync<CommunityViewModel>(query);
                result[i].CommunitiesOfPost = community.ToList();
            }
            return result;
        }
        public async Task<IEnumerable<PostViewModel>> GetPostsForFeed(int offset)
        {
            List<PostViewModel> result = new List<PostViewModel>();
            var query = $"SELECT * " +
                $"FROM dbo.Posts WHERE dbo.Posts.isDeleted =0 " +
                $"ORDER BY dbo.Posts.CreatedAt DESC OFFSET {offset*10} ROWS FETCH NEXT 10 ROWS ONLY;";

            var response = await _service.GetDataAsync<PostViewModel>(query);
            if (response.Count == 0) return result;

            result = response.ToList();
            for (int i = 0; i<result.Count; i++)
            {
                query = $"SELECT Users.* " +
                    $"FROM [dbo].[Posts] " +
                    $"INNER JOIN PostLikeUser ON Posts.ID=PostLikeUser.PostId " +
                    $"INNER JOIN Users ON PostLikeUser.UserName=Users.UserName " +
                    $"WHERE Posts.ID='{result[i].ID}'";
                var users = await _service.GetDataAsync<UserViewModel>(query);
                result[i].UsersLikePost = users.ToList();
                query = $"SELECT Community.*" +
                    $"FROM [dbo].[Posts] " +
                    $"INNER JOIN PostWithCommunity ON Posts.ID=PostWithCommunity.PostId " +
                    $"INNER JOIN Community ON PostWithCommunity.CommunityId=Community.ID " +
                    $"WHERE Posts.ID='{result[i].ID}'";
                var community = await _service.GetDataAsync<CommunityViewModel>(query);
                result[i].CommunitiesOfPost = community.ToList();
            }
            return result;
        }

        public async Task<Guid> UpdatePost(PostUpdateModel model)
        {
            bool flag=false;
            var query = $"UPDATE dbo.Posts SET ";
            if (model.Title != null)
            {
                query += $"Title=N'{model.Title}'";
                flag = true;
            }
            if (model.Contents != null)
            {
                if (flag) query += $", Contents=N'{model.Contents}'";
                else query += $"Contents=N'{model.Contents}'";
                flag = true;
            }
            if (model.Price != null)
            {
                if(flag) query += $", Price=N'{model.Price}'";
                else query += $" Price=N'{model.Price}'";
                flag = true;
            }
            if (model.Category != null)
            {
                if(flag) query += $", Category=N'{model.Category}'";
                else query += $" Category=N'{model.Category}'";
            }
            query += $", UpdatedAt=CURRENT_TIMESTAMP WHERE ID='{model.Id}'";
            await _service.GetDataAsync(query);

            return model.Id;
        }

        public async Task<Guid> DeletePost(Guid postId)
        {
            var query = $"DELETE FROM dbo.PostWithCommunity WHERE PostId='{postId}';" +
                $"DELETE FROM dbo.PostLikeUser WHERE PostId='{postId}';" +
                $"UPDATE dbo.Posts SET isDeleted=1 WHERE ID='{postId}';";
            var response = await _service.GetDataAsync(query);
            //GlobalModule.NumberOfPosts[response[0].Id.ToString()] = (int)GlobalModule.NumberOfPosts[response[0].Id.ToString()] - 1;
            return postId;
        }

        public async Task<IEnumerable<CommunityViewModel>> GetJoinedCommunity(string username)
        {
            var query = $"SELECT dbo.Community.ID,dbo.Community.CommunityName,dbo.Community.CommunityDescription,dbo.Community.CommunityOwnerName,dbo.Community.CommunityType,dbo.Community.CreatedAt,dbo.Community.UpdatedAt,dbo.Community.ForegroundImage,dbo.Community.BackgroundImage,dbo.Community.NumberOfUsers,dbo.Community.NumberOfPosts,dbo.Community.NumberOfActiveUsers " +
                $"FROM dbo.Community " +
                $"INNER JOIN dbo.CommunityMember ON dbo.CommunityMember.CommunityID =dbo.Community.ID " +
                $"WHERE dbo.CommunityMember.UserName =N'{username}'";
            var response = await _service.GetDataAsync<CommunityViewModel>(query);
            return response.ToList();
        }
        public async Task<IEnumerable<CommunityViewModel>> GetCommunityForFeed(int offset)
        {
            var query = $"SELECT dbo.Community.ID,dbo.Community.CommunityName,dbo.Community.CommunityDescription,dbo.Community.CommunityOwnerName,dbo.Community.CommunityType,dbo.Community.CreatedAt,dbo.Community.UpdatedAt,dbo.Community.ForegroundImage,dbo.Community.BackgroundImage,dbo.Community.NumberOfUsers,dbo.Community.NumberOfPosts,dbo.Community.NumberOfActiveUsers " +
                $"FROM dbo.Community " +
                $"ORDER BY dbo.Community.CreatedAt DESC OFFSET {offset*10} ROWS FETCH NEXT 10 ROWS ONLY;";
            var response = await _service.GetDataAsync<CommunityViewModel>(query);
            return response.ToList();
        }
        public async Task<string> JoinCommunity(string username, Guid communityId)
        {
            var query = $"INSERT INTO dbo.CommunityMember VALUES(NEWID(), N'{username}','{communityId}',2);";
            await _service.GetDataAsync(query);
            return $"{username} Joined Community.";
        }

        public async Task<string> ExitCommunity(string username, Guid communityId)
        {
            var query = $"DELETE FROM dbo.CommunityMember WHERE UserName=N'{username} AND CommunityID='{communityId}';";
            await _service.GetDataAsync(query);
            return $"{username} exited from Community.";
        }

        public async Task<CommunityMember> GetUserRole(string username, Guid communityId)
        {
            var query = $"SELECT * FROM dbo.CommunityMember WHERE UserName=N'{username}' AND CommunityID='{communityId}';";
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
            var query = $"INSERT INTO dbo.Event OUTPUT Inserted.ID VALUES(NEWID(), N'{e.Title}', N'{e.HostName}', ";
            
            if (e.Description != null) query += $"N'{e.Description}', ";
            else query += $"NULL, ";

            if (e.Time != null) query += $"'{e.Time}',";
            else query += $"NULL, ";

            if (e.Access != null) query += $"N'{e.Access}', ";
            else query += $"NULL, ";

            if (e.Image != null) query += $"N'{e.Image}', ";
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
                $"SET Title=N'{e.Title}', Description=N'{e.Description}', Time='{e.Time}', Access=N'{e.Access}',Image=N'{e.Image}', Limit={e.Limit}, UpdatedAt=CURRENT_TIMESTAMP " +
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

        public async Task<string> AddAdmin(string username, Guid communityId)
        {
            var query = $"UPDATE CommunityMember SET UserRole=1 WHERE UserName=N'{username}' AND CommunityID='{communityId}';";
            await _service.GetDataAsync(query);
            return $"{username} is selected as an admin of community!";
        }

        public async Task<string> RemoveAdmin(string username, Guid communityId)
        {
            var query = $"UPDATE CommunityMember SET UserRole=2 WHERE UserName=N'{username}' AND CommunityID='{communityId}';";
            await _service.GetDataAsync(query);
            return $"{username} is removed from admin of community!";
        }

        public async Task<string> JoinChannel(string name, Guid channelId)
        {
            var query = $"INSERT INTO [dbo].[ChannelMember] VALUES (NEWID(), '{name}','{channelId}',0);";
            await _service.GetDataAsync(query);
            return "New User Joined";
        }

        public async Task<string> ExitChannel(string name, Guid channelId)
        {
            var query = $"INSERT INTO [dbo].[ChannelMember] VALUES (NEWID(), '{name}','{channelId}',0);";
            await _service.GetDataAsync(query);
            return "User exited";
        }

        public async Task UpdateUserNumberOfCommunity(int v, Guid communityId)
        {
            var query = $"UPDATE [dbo].[Community] SET NumberOfUsers={v} WHERE ID='{communityId}';";
            await _service.GetDataAsync(query);
        }

        public async Task<SearchResultViewModel> GetSearchResult(string text)
        {
            SearchResultViewModel result = new SearchResultViewModel();
            var query = $"SELECT * FROM Community WHERE CHARINDEX(N'{text}', CommunityName) > 0";
            var response = await _service.GetDataAsync<CommunityViewModel>(query);
            result.communityViewModels=response.ToList();

            query = $"SELECT * FROM Users WHERE CHARINDEX(N'{text}', UserName) > 0";
            var response1 = await _service.GetDataAsync<UserViewModel>(query);
            result.usersViewModels = response1.ToList();

            query = $"SELECT * FROM Posts WHERE CHARINDEX(N'{text}', Title) > 0";
            var response2 = await _service.GetDataAsync<PostViewModel>(query);
            result.postsViewModels = response2.ToList();

            query = $"SELECT * FROM Event WHERE CHARINDEX(N'{text}', Title) > 0";
            var response3 = await _service.GetDataAsync<EventViewModel>(query);
            result.eventsViewModels = response3.ToList();
            return result;
        }

        public async Task<string> LikePost(string userName, Guid postId)
        {
            var query = $"INSERT INTO PostLikeUser VALUES(NEWID(), '{postId}', '{userName}')";
            await _service.GetDataAsync(query);

            return $"{userName} like post";
        }

        public async Task<string> UnLikePost(string userName, Guid postId)
        {
            var query = $"DELETE FROM PostLikeUser WHERE PostId='{postId}' AND UserName='{userName}';";
            await _service.GetDataAsync(query);

            return $"{userName} unlike post";
        }
    }
}
