using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class UserConnectionService : IUserConnectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public UserConnectionService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<UserConnection?> CreateUserConnectionAsync(int userId, string connectionId)
        {
            if (connectionId == null)
                return null;


            if (await _userService.CheckAnyAsync(userId) == false)
                return null;

            var connection = new UserConnection
            {
                UserId = userId,
                ConnectionId = connectionId
            };

            await _unitOfWork.UserConnections.AddOrUpdateAsync(connection);
            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
                return null;

            return connection;
        }

        public async Task<UserConnection?> DisconnectAsync(string? connectionId)
        {
            if(connectionId == null)
                return null;

            var connection = await _unitOfWork.UserConnections.FindAsync(c => c.ConnectionId == connectionId);
            if (connection == null)
                return null;

            connection.DisconnectedAt = DateTime.UtcNow;
            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0) 
                return null;

            return connection;
        }

        public async Task<List<UserConnection>> GetAllUserConnectionsAsync(int userId)
        {
            return (await _unitOfWork.UserConnections.GetAllAsync(c => c.UserId == userId)).ToList();
        }

        public async Task<bool> RemoveConnectionIdAsync(string? connectionId)
        {
            if (connectionId == null)
                return false;

            var connection = await _unitOfWork.UserConnections.FindAsync(c => c.ConnectionId == connectionId);
            if(connection == null)
                return false;

            _unitOfWork.UserConnections.Delete(connection);
            var changes = await _unitOfWork.CommitAsync();

            return changes > 0;
        }
    }
}
