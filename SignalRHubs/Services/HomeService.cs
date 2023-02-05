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
        public HomeService(IDapperService<Community> dapperService)
        {
            _service = dapperService;
        }

        public async Task<Guid> CreateChannel(Channel entity)
        {
            var query = $"INSERT INTO Channel OUTPUT Inserted.ChannelId as Id VALUES(" +
                $"NEWID(), " +
                $"@cName, ";

            if (entity.ChannelDescription != null) query += $"@cDes, ";
            else query += $"NULL, ";

            query+= $"@cOwner, " +
                $"@cComId, " +
                $"CURRENT_TIMESTAMP," +
                $"NULL" +
                $")";
            var obj = new
            {
                cName = entity.ChannelName,
                cDes = entity.ChannelDescription,
                cOwner = entity.ChannelOwnerName,
                cComId = entity.ChannelCommunityId
            };
            var res = await _service.GetDataAsync(query, obj);
            //update channelmember
            query = $"INSERT INTO [dbo].[ChannelMember] VALUES (NEWID(), @cOwner,@ID,1);";
            await _service.GetDataAsync(query, new { cOwner = entity.ChannelOwnerName, ID = res[0].Id});

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
                $"@eId, " +
                $"eComName, ";

            if (entity.CommunityDescription == null) query += $"NULL, ";
            else query += $"@eComDes, ";

            query += $"@eComOwner, ";

            if (entity.CommunityType == null) query += $"NULL, ";
            else query += $"@eType, ";

            query += $"CURRENT_TIMESTAMP, " + $"NULL,";

            if (entity.ForegroundImage == null) query += $"NULL, ";
            else query += $"@eForImg, ";

            if (entity.BackgroundImage == null) query += $"NULL, ";
            else query += $"@eBackImg, ";

            query += $"0, 0, 0);";
            var obj = new
            {
                eId = entity.Id,
                eComName = entity.CommunityName,
                eComDes = entity.CommunityDescription,
                eComOwner = entity.CommunityOwnerName,
                eType = entity.CommunityType,
                eForImg = entity.ForegroundImage,
                eBackImg=entity.BackgroundImage
            };
            await _service.GetDataAsync(query, obj);

            // Update CommunityMember Table
            query = $"INSERT INTO dbo.CommunityMember VALUES(NEWID(), @eComOwner,@eId,0);";
            await _service.GetDataAsync(query, new {eComOwner = entity.CommunityOwnerName, eId = entity.Id});

            return entity.Id;
        }

        public async Task<string> UpdateCommunityAvatar(Guid id, string url)
        {
            var query = $"UPDATE [dbo].[Community] SET ForegroundImage = @Url WHERE ID = @Id";
            await _service.GetDataAsync(query, new {Url = url, Id = id});
            return "Successfully updated community avatar!";
        }

        public async Task<string> UpdateCommunityBackGround(Guid id, string url)
        {
            var query = $"UPDATE [dbo].[Community] SET BackgroundImage = @Url WHERE ID = @Id";
            await _service.GetDataAsync(query, new { Url = url, Id = id });
            return "Successfully updated community background!";
        }

        public async Task<Guid> DeleteChannel(Guid id)
        {
            // Update referenced table ChannelMember, message, event
            var query = $"DELETE FROM dbo.ChannelMember WHERE ChannelId=@Id; " +
                $"DELETE dbo.ChannelMessage WHERE ChannelId = @Id;" +
                $"DELETE FROM dbo.Channel WHERE ChannelId = @Id; "; 

            await _service.GetDataAsync(query, new {Id = id});
            return id;
        }  

        public async Task<Guid> DeleteCommunity(Guid id)
        {
            var query = $"DECLARE @Counter INT, @i INT " +
                $"WITH x as (SELECT* FROM dbo.Channel WHERE dbo.Channel.ChannelCommunityId = @Id) " +
                $"SELECT @Counter = COUNT(*) FROM x " +
                $"SET @i = 1 " +
                $"WHILE(@i <= @Counter) " +
                $"BEGIN " +
                $"DECLARE @Id uniqueidentifier " +
                $"SELECT TOP 1 @Id = ChannelId FROM(SELECT TOP(@i) * FROM dbo.Channel WHERE dbo.Channel.ChannelCommunityId = @Id ORDER BY ChannelId ASC) tmp ORDER BY ChannelId DESC " +
                $"DELETE FROM dbo.ChannelMember WHERE ChannelId = @Id; " +
                $"DELETE dbo.ChannelMessage WHERE ChannelId = @Id;" +
                $"DELETE FROM dbo.Channel WHERE ChannelId = @Id; " +
                $"PRINT 'The counter value is = ' + CONVERT(nvarchar(36), @Id) " +
                $"SET @i = @i + 1 " +
                $"END " +
                $"DELETE FROM dbo.CommunityMember WHERE CommunityID = @Id; " +
                $"DELETE FROM dbo.PostWithCommunity WHERE CommunityId = @Id; " +
                $"UPDATE dbo.Event SET CommunityId = NULL, isDeleted = 1 WHERE CommunityId = @Id; " +
                $"DELETE FROM dbo.Community WHERE ID = @Id; ";

            await _service.GetDataAsync(query, new { Id = id });

            return id;
        }

        public async Task<IEnumerable<ChannelViewModel>> GetAllChannels(Guid communityID)
        {
            var query = $"SELECT * FROM dbo.Channel WHERE ChannelCommunityId = @Id;";
            var response = await _service.GetDataAsync<ChannelViewModel>(query, new {Id = communityID});
            return response.ToList();
        }

        public async Task<Guid> UpdateChannel(ChannelUpdateModel model)
        {
            var query = $"UPDATE dbo.Channel SET ";
            if (model.ChannelName != null) query += $"ChannelName=@mCname";
            if (model.ChannelDescription != null) query += $",ChannelDescription = @mCDes";

            query += $", UpdatedAt = CURRENT_TIMESTAMP WHERE dbo.Channel.ChannelId = @mId;";

            var obj = new
            {
                mCname = model.ChannelName,
                mCDes = model.ChannelDescription,
                mId = model.ChannelId
            };
            var response = await _service.GetDataAsync(query, obj);
            return model.ChannelId;
        }

        public async Task<Guid> UpdateCommunity(Community entity)
        {
            bool flag = false;
            var query = $"UPDATE dbo.Community SET ";
            if (entity.CommunityName != null)
            {
                query += $"CommunityName=@eComName";
                flag = true;
            }
            if (entity.CommunityDescription != null)
            {
                if (flag) query += $", ";
                query += $"CommunityDescription = @eComDes";
                flag = true;
            }
            if (entity.CommunityType != null)
            {
                if (flag) query += $", ";
                query += $" CommunityType = @eComType";
                flag = true;
            }
            if (flag) query += $", ";
            query += $" UpdatedAt = CURRENT_TIMESTAMP";

            if (entity.ForegroundImage != null) query += $", ForegroundImage = @eForImag";
            if (entity.BackgroundImage != null) query += $", BackgroundImage = @eBackImg";
            if (entity.NumberOfUsers != null) query += $", NumberOfUsers = @eNumUser";
            if (entity.NumberOfPosts != null) query += $", NumberOfPosts = @eNumPost";
            if (entity.NumberOfActiveUsers != null) query += $", NumberOfActiveUsers = @eNumActive";
            query += $" WHERE dbo.Community.ID = @eId; ";

            var obj = new { eComName = entity.CommunityName, eComDes = entity.CommunityDescription,
            eComType = entity.CommunityType, eForImag = entity.ForegroundImage, eBackImg = entity.BackgroundImage,
            eNumUser = entity.NumberOfUsers, eNumPost = entity.NumberOfPosts, eNumActive = entity.NumberOfActiveUsers,
            eId = entity.Id};
            await _service.GetDataAsync(query, obj);  

            return entity.Id;
        }

        public async Task<Guid> CreatePost(Post entity, PostCreateModel model)
        {
            var query = $"INSERT INTO dbo.Posts OUTPUT Inserted.ID Values(NEWID(), @eUname, @eTitle, ";
            if (entity.Contents != null) query += $"@eCon,";
            else query += $"NULL, ";

            if (entity.Price != null) query += $"@ePrice,";
            else query += $"NULL, ";

            if (entity.Category != null) query += $"@eCategory,";
            else query += $"NULL, ";

            query += $"CURRENT_TIMESTAMP, NULL, 'FALSE')";
            var obj = new { eUname = entity.UserName, eTitle = entity.Title, eCon = entity.Contents, ePrice = entity.Price,
                eCategory = entity.Category };
            var response = await _service.GetDataAsync<Post>(query, obj);

            query = "";
            var param = new { Id = response[0].Id };

            if (model.CommunityIds != null)
            {
                for (int i = 0; i < model.CommunityIds.Count; i++)
                {
                    query += $"INSERT INTO dbo.PostWithCommunity VALUES(NEWID(), @Id, '{model.CommunityIds[i]}');";
                }
            }
            if(model.Urls != null)
            {
                for (int i = 0; i < model.Urls.Count; i++)
                {
                    query += $"INSERT INTO dbo.PostFile VALUES(NEWID(), @Id, {i}, N'{model.Urls[i]}', N'{model.Types[i]}', {model.PreviewWs[i]}, {model.PreviewHs[i]})";
                }
            }
            if(query!="") await _service.GetDataAsync(query, param);

            return response[0].Id;
        }

        public async Task<IEnumerable<PostViewModel>> GetPosts(string username)
        {
            List<PostViewModel> result = new List<PostViewModel>();
            var query = $"SELECT * " +
                $"FROM dbo.Posts WHERE dbo.Posts.UserName =@name AND dbo.Posts.isDeleted =0 " +
                $"ORDER BY dbo.Posts.CreatedAt DESC";
            var response = await _service.GetDataAsync<PostViewModel>(query, new {name = username});
            if (response.Count == 0) return result;

            result = response.ToList();
            for(int i=0; i<result.Count; i++)
            {
                query = $"SELECT * " +
                    $"FROM dbo.PostFile " +
                    $"WHERE dbo.PostFile.PostId=@Id" +
                    $"ORDER BY [Index] ASC;";
                var postfiles = await _service.GetDataAsync<PostFileViewModel>(query, new {Id = result[i].ID});
                result[i].PostFiles = postfiles.ToList();
                query = $"SELECT Users.* " +
                    $"FROM [dbo].[Posts] " +
                    $"INNER JOIN PostLikeUser ON Posts.ID=PostLikeUser.PostId " +
                    $"INNER JOIN Users ON PostLikeUser.UserName=Users.UserName " +
                    $"WHERE Posts.ID=@Id";
                var users = await _service.GetDataAsync<UserViewModel>(query, new { Id = result[i].ID });
                result[i].UsersLikePost = users.ToList();
                query = $"SELECT Community.*" +
                    $"FROM [dbo].[Posts] " +
                    $"INNER JOIN PostWithCommunity ON Posts.ID=PostWithCommunity.PostId " +
                    $"INNER JOIN Community ON PostWithCommunity.CommunityId=Community.ID " +
                    $"WHERE Posts.ID=@Id";
                var community = await _service.GetDataAsync<CommunityViewModel>(query, new { Id = result[i].ID });
                result[i].CommunitiesOfPost = community.ToList();
            }
            return result;
        }
        public async Task<IEnumerable<PostFeedViewModel>> GetPostsForFeed(int offset)
        {
            List<PostFeedViewModel> result = new List<PostFeedViewModel>();
            var query = $"SELECT * " +
                $"FROM dbo.Posts WHERE dbo.Posts.isDeleted =0 " +
                $"ORDER BY dbo.Posts.CreatedAt DESC OFFSET @ofs ROWS FETCH NEXT 10 ROWS ONLY;";

            var response = await _service.GetDataAsync<PostFeedViewModel>(query, new {ofs = offset*10});
            if (response.Count == 0) return result;

            result = response.ToList();
            for (int i = 0; i<result.Count; i++)
            {
                query = $"SELECT * FROM dbo.Users WHERE UserName = @name;";
                var postowner = await _service.GetDataAsync<UserViewModel>(query, new {name = result[i].UserName});
                if(postowner != null) result[i].PostOwner = postowner[0];

                query = $"SELECT * " +
                    $"FROM dbo.PostFile " +
                    $"WHERE dbo.PostFile.PostId=@Id " +
                    $"ORDER BY [Index] ASC;";
                var postfiles = await _service.GetDataAsync<PostFileViewModel>(query, new {Id = result[i].ID});
                result[i].PostFiles = postfiles.ToList();
                query = $"SELECT Users.* " + 
                    $"FROM [dbo].[Posts] " +
                    $"INNER JOIN PostLikeUser ON Posts.ID=PostLikeUser.PostId " +
                    $"INNER JOIN Users ON PostLikeUser.UserName=Users.UserName " +
                    $"WHERE Posts.ID=@Id";
                var users = await _service.GetDataAsync<UserViewModel>(query, new { Id = result[i].ID });
                result[i].UsersLikePost = users.ToList();
                query = $"SELECT Community.*" +
                    $"FROM [dbo].[Posts] " +
                    $"INNER JOIN PostWithCommunity ON Posts.ID=PostWithCommunity.PostId " +
                    $"INNER JOIN Community ON PostWithCommunity.CommunityId=Community.ID " +
                    $"WHERE Posts.ID=@Id";
                var community = await _service.GetDataAsync<CommunityViewModel>(query, new { Id = result[i].ID });
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
                query += $"Title=@mTitle ";
                flag = true;
            }
            if (model.Contents != null)
            {
                if (flag) query += $", Contents=@mContent ";
                else query += $"Contents=@mContent ";
                flag = true;
            }
            if (model.Price != null)
            {
                if(flag) query += $", Price=@mPrice ";
                else query += $" Price=@mPrice ";
                flag = true;
            }
            if (model.Category != null)
            {
                if(flag) query += $", Category=@mCategory";
                else query += $" Category=@mCategory";
            }
            query += $", UpdatedAt=CURRENT_TIMESTAMP WHERE ID=@mId";
            if (model.CommunityIds != null)
            {
                query += $"DELETE * FROM PostWithCommunity WHERE PostId=@mId;";
                for(int i =0; i<model.CommunityIds.Count; i++)
                {
                    query += $"INSERT INTO PostWithCommunity VALUES(NEWID(), @mId, '{model.CommunityIds[i]}');";
                }
            }
            if(model.Urls != null)
            {
                query += $"DELETE * FROM PostFile WHERE PostId=@mId;";
                for (int i=0; i<model.Urls.Count; i++)
                {
                    query += $"INSERT INTO PostFile VALUES(NEWID(), @mId, {i}, '{model.Urls[i]}', '{model.Types[i]}', {model.PreviewWs[i]}, {model.PreviewHs[i]});";
                }
            }
            var param = new
            {
                mTitle = model.Title,
                mContent = model.Contents,
                mPrice = model.Price,
                mCategory = model.Category,
                mId = model.Id
            };
            await _service.GetDataAsync(query, param);

            return model.Id;
        }

        public async Task<Guid> DeletePost(Guid postId)
        {
            var query = $"DELETE FROM dbo.PostWithCommunity WHERE PostId=@Id;" +
                $"DELETE FROM dbo.PostLikeUser WHERE PostId=@Id;" +
                $"DELETE FROM dbo.PostFile WHERE PostId=@Id;" +
                $"UPDATE dbo.Posts SET isDeleted=1 WHERE ID=@Id;";
            var response = await _service.GetDataAsync(query, new {Id = postId});
            return postId;
        }

        public async Task<IEnumerable<CommunityViewModel>> GetJoinedCommunity(string username)
        {
            var query = $"SELECT dbo.Community.ID,dbo.Community.CommunityName,dbo.Community.CommunityDescription,dbo.Community.CommunityOwnerName,dbo.Community.CommunityType,dbo.Community.CreatedAt,dbo.Community.UpdatedAt,dbo.Community.ForegroundImage,dbo.Community.BackgroundImage,dbo.Community.NumberOfUsers,dbo.Community.NumberOfPosts,dbo.Community.NumberOfActiveUsers " +
                $"FROM dbo.Community " +
                $"INNER JOIN dbo.CommunityMember ON dbo.CommunityMember.CommunityID =dbo.Community.ID " +
                $"WHERE dbo.CommunityMember.UserName =@name " +
                $"ORDER BY NumberOfUsers DESC";
            var response = await _service.GetDataAsync<CommunityViewModel>(query, new {name = username});
            return response.ToList();
        }
        public async Task<IEnumerable<CommunityViewModel>> GetCommunityForFeed(int offset)
        {
            var query = $"SELECT dbo.Community.ID,dbo.Community.CommunityName,dbo.Community.CommunityDescription,dbo.Community.CommunityOwnerName,dbo.Community.CommunityType,dbo.Community.CreatedAt,dbo.Community.UpdatedAt,dbo.Community.ForegroundImage,dbo.Community.BackgroundImage,dbo.Community.NumberOfUsers,dbo.Community.NumberOfPosts,dbo.Community.NumberOfActiveUsers " +
                $"FROM dbo.Community " +
                $"ORDER BY dbo.Community.NumberOfUsers DESC OFFSET @ofs ROWS FETCH NEXT 10 ROWS ONLY;";
            var response = await _service.GetDataAsync<CommunityViewModel>(query, new {ofs = offset*10});
            return response.ToList();
        }
        public async Task<string> JoinCommunity(string username, Guid communityId)
        {
            var query = $"INSERT INTO dbo.CommunityMember VALUES(NEWID(), @name,@Id,2);";
            await _service.GetDataAsync(query, new {name = username, Id = communityId});
            return $"{username} Joined Community.";
        }

        public async Task<string> ExitCommunity(string username, Guid communityId)
        {
            var query = $"DELETE FROM dbo.CommunityMember WHERE UserName=@name AND CommunityID=@Id;";
            await _service.GetDataAsync(query, new { name = username, Id = communityId });
            return $"{username} exited from Community.";
        }

        public async Task<CommunityMember> GetUserRole(string username, Guid communityId)
        {
            var query = $"SELECT * FROM dbo.CommunityMember WHERE UserName=@name AND CommunityID=@Id;";
            var response = await _service.GetDataAsync<CommunityMember>(query, new { name = username, Id = communityId });

            CommunityMember m = new CommunityMember();
            m.UserRole = 4;
            if (response.Count == 0) return m;
            return response[0];
        }

        public async Task<Channel> GetChannelById(Guid channelId)
        {
            var query = $"SELECT * FROM dbo.Channel WHERE ChannelId=@Id;";
            var response = await _service.GetDataAsync<Channel>(query, new { Id = channelId });

            if (response.Count == 0) return null;
            return response[0];
        }

        public async Task<Community> GetCommunityById(Guid id)
        {
            var query = $"SELECT*FROM dbo.Community WHERE ID =@Id";
            var response = await _service.GetDataAsync(query, new {Id = id});
            if (response.Count == 0) return null;
            return response[0];
        }

        public async Task<Guid> CreateEvent(Event e)
        {
            var query = $"INSERT INTO dbo.Event OUTPUT Inserted.ID VALUES(NEWID(), @eTitle, @eHostName, ";
            
            if (e.Description != null) query += $"@eDescription, ";
            else query += $"NULL, ";

            if (e.Time != null) query += $"@eTime,";
            else query += $"NULL, ";

            if (e.Access != null) query += $"@eAccess, ";
            else query += $"NULL, ";

            if (e.Image != null) query += $"@eImage, ";
            else query += $"NULL, ";

            if (e.Limit != null) query += $"@eLimit, ";
            else query += $"NULL, ";

            query += $"@eId, CURRENT_TIMESTAMP, NULL, 0);";
            var param = new
            {
                eTitle = e.Title,
                eHostName = e.HostName,
                eDescription = e.Description,
                eTime = e.Time,
                eAccess = e.Access,
                eImage = e.Image,
                eLimit = e.Limit,
                eId = e.CommunityId
            };
            var response = await _service.GetDataAsync<Event>(query, param);
            if (response.Count == 0) return Guid.Empty;

            return response[0].Id;
        }

        public async Task<Event> GetEventByID(Guid id)
        {
            var query = $"SELECT * FROM dbo.Event WHERE Id=@Id";
            var response = await _service.GetDataAsync<Event>(query, new {Id = id});
            if (response.Count == 0) return null;
            return response[0];
        }

        public async Task<Guid> UpdateEvent(Event e)
        {
            var query = $"UPDATE dbo.Event " +
                $"SET Title=@eTitle, Description=@eDes, Time=@eTime, Access=@eAccess,Image=@eImage, Limit=@eLimit, UpdatedAt=CURRENT_TIMESTAMP " +
                $"WHERE ID='{e.Id}'";
            var param = new 
            {
                eTitle = e.Title,
                eDes = e.Description,
                eTime = e.Time,
                eAccess = e.Access,
                eImage = e.Image,
                eLimit = e.Limit
            };
            var response =await _service.GetDataAsync<Event>(query, param);
            return e.Id;
        }

        public async Task<Guid> DeleteEvent(Guid id)
        {
            var query = $"UPDATE dbo.Event SET isDeleted=1 WHERE ID=@Id";
            await _service.GetDataAsync(query, new {Id = id});

            return id;
        }

        public async Task<IEnumerable<EventViewModel>> GetAllEvent(Guid communityId)
        {
            var query = $"SELECT * FROM dbo.Event Where CommunityId=@Id AND isDeleted=0;";
            var response = await _service.GetDataAsync<EventViewModel>(query, new {Id = communityId});

            return response.ToList();
        }

        public async Task<Community> GetCommunityByChannelId(Guid channelId)
        {
            var query = $"SELECT*FROM dbo.Community INNER JOIN dbo.Channel ON dbo.Channel.ChannelCommunityId =dbo.Community.ID WHERE dbo.Channel.ChannelId=@Id;";
            var response = await _service.GetDataAsync<Community>(query, new {Id = channelId});
            if (response.Count == 0) return null;
            return response[0];
        }

        public async Task<string> AddAdmin(string username, Guid communityId)
        {
            var query = $"UPDATE CommunityMember SET UserRole=1 WHERE UserName=@name AND CommunityID=@Id;";
            await _service.GetDataAsync(query, new {name = username, Id = communityId});
            return $"{username} is selected as an admin of community!";
        }

        public async Task<string> RemoveAdmin(string username, Guid communityId)
        {
            var query = $"UPDATE CommunityMember SET UserRole=2 WHERE UserName=@name AND CommunityID=@Id;";
            await _service.GetDataAsync(query, new { name = username, Id = communityId });
            return $"{username} is removed from admin of community!";
        }

        public async Task<string> JoinChannel(string name, Guid channelId)
        {
            var query = $"INSERT INTO [dbo].[ChannelMember] VALUES (NEWID(), @Name,@Id,0);";
            await _service.GetDataAsync(query, new {Name = name, Id = channelId});
            return "New User Joined";
        }

        public async Task<string> ExitChannel(string name, Guid channelId)
        {
            var query = $"INSERT INTO [dbo].[ChannelMember] VALUES (NEWID(), @Name,@Id,0);";
            await _service.GetDataAsync(query, new { Name = name, Id = channelId });
            return "User exited";
        }

        public async Task UpdateUserNumberOfCommunity(int v, Guid communityId)
        {
            var query = $"UPDATE [dbo].[Community] SET NumberOfUsers=@num WHERE ID=@Id;";
            await _service.GetDataAsync(query, new {num = v, Id = communityId});
        }

        public async Task<SearchResultViewModel> GetSearchResult(string text)
        {
            SearchResultViewModel result = new SearchResultViewModel();
            var query = $"SELECT * FROM Community WHERE CHARINDEX(@keyword, CommunityName) > 0";
            var response = await _service.GetDataAsync<CommunityViewModel>(query, new {keyword = text});
            result.communityViewModels=response.ToList();

            query = $"SELECT * FROM Users WHERE CHARINDEX(@keyword, UserName) > 0";
            var response1 = await _service.GetDataAsync<UserViewModel>(query, new { keyword = text });
            result.usersViewModels = response1.ToList();

            query = $"SELECT * FROM Posts WHERE CHARINDEX(@keyword, Title) > 0";
            var response2 = await _service.GetDataAsync<PostViewModel>(query, new { keyword = text });
            result.postsViewModels = response2.ToList();

            query = $"SELECT * FROM Event WHERE CHARINDEX(@keyword, Title) > 0";
            var response3 = await _service.GetDataAsync<EventViewModel>(query, new { keyword = text });
            result.eventsViewModels = response3.ToList();

            return result;
        }

        public async Task<string> LikePost(string userName, Guid postId)
        {
            var query = $"INSERT INTO PostLikeUser VALUES(NEWID(), @name, @Id)";
            await _service.GetDataAsync(query, new {name = userName, Id = postId });

            return $"{userName} like post";
        }

        public async Task<string> UnLikePost(string userName, Guid postId)
        {
            var query = $"DELETE FROM PostLikeUser WHERE PostId=@Id AND UserName=@name;";
            await _service.GetDataAsync(query, new { name = userName, Id = postId });

            return $"{userName} unlike post";
        }
    }
}
