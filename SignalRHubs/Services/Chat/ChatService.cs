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

        public async Task<List<ChatRoomViewModel>> GetChatRoomsByUserId(Guid userId)
        {
            var query = @"
                With R As 
                (   
                    Select RoomId
					From ChatRoomDetails 
					Where UserId = @UserId
					GROUP BY RoomId
                ),
				RU AS
				(
					Select R.RoomId, UserId
					From R
					INNER JOIN ChatRoomDetails CRD
					ON CRD.RoomId = R.RoomId
					Where UserId != @UserId
					GROUP BY R.RoomId, UserId
				)
					
                Select RU.RoomId, U.Id ReceiverId, U.FirstName + ' ' + U.LastName ReceiverName 
                From RU
                Inner Join Users U On RU.UserId = U.Id 
                Group By RU.RoomId, U.Id, U.FirstName, U.LastName;";

            return await _service.GetDataAsync<ChatRoomViewModel>(query, new {UserId = userId});
        } 

        public async Task<Tuple<Guid?, IEnumerable<string>>> GetChatRoomByUserIds(IEnumerable<string> userIds)
        {
            var query = @"
                Select RoomId, Count(*) Over() TotalUsers
                From ChatRoomDetails 
                Where UserId In @UserIds;

                Select FirstName + ' ' + LastName Name
                From Users 
                Where Id In @UserIds";

            using SqlConnection uow = _service.Connection;
            await uow.OpenAsync();
            var records = await uow.QueryMultipleAsync(query, new { UserIds = userIds });

            var rec = await records.ReadFirstOrDefaultAsync();
            Guid? roomId = null;
            
            if (rec != null)
            {
                var totalUsers = rec.TotalUsers;
                if (totalUsers == userIds.Count()) roomId = (Guid?)rec.RoomId;
            }

            var userNames = await records.ReadAsync<string>();

            return new Tuple<Guid?, IEnumerable<string>>(roomId, userNames);
        }

        public async Task<Message> GetMessage(Guid Id)
        {
            var query = @"
                Select Id, SenderId, Content, RoomId, CreatedAt, UpdatedAt --, U.FirstName + ' ' + U.LastName Name
                From Messages M
                --Inner Join Users U On M.SenderId = U.Id
                Where Id = @Id";
            return await _service.GetFirstOrDefaultAsync<Message>(query, new { Id = Id });
        }

        public async Task<List<MessageViewModel>> GetMessageByRoomId(Guid roomId)
        {
            var query = @"Select Id, SenderId, Content, RoomId, CreatedAt, UpdatedAt --, U.FirstName + ' ' + U.LastName Name
                From Messages M
                --Inner Join Users U On M.SenderId = U.Id
                Where RoomId = @roomId";
            return await _service.GetDataAsync<MessageViewModel>(query, new { RoomId = roomId });
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

        public async Task SaveRoom(ChatRoom entity)
        {
            SqlTransaction? transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _service.SaveAsync(entity, _connection, transaction);
                var details = entity.Details.ToList();
                await _service.SaveManyAsync<ChatRoomDetail>(details, _connection, transaction);

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
    }
}