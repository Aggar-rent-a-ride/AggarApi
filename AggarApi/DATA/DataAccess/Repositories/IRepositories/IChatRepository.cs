using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Repositories.IRepositories
{
    public interface IChatRepository : IBaseRepository<Message>
    {
        Task<IEnumerable<Message>> GetChatAsync(int authUserId, int pageNo, int pageSize);
        Task<IEnumerable<int>> GetLatestUnseenMessagesIds(int userId1, int userId2);
    }
}
