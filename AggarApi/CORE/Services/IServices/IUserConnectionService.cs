using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IUserConnectionService
    {
        //index connectionid
        Task<UserConnection?> CreateUserConnectionAsync(int userId, string connectionId);
        Task<List<UserConnection>> GetAllUserConnectionsAsync(int userId);
        Task<bool> RemoveConnectionIdAsync(string? connectionId);
        Task<UserConnection?> DisconnectAsync(string? connectionId);
    }
}
