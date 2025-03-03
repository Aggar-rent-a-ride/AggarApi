using DATA.Constants.Enums;
using DATA.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Repositories.IRepositories
{
    public interface IChatRepository : IBaseRepository<Message>
    {
        Task<IEnumerable<Message>> GetLatestChatMessagesAsync(int authUserId, int pageNo, int pageSize);
        Task<IEnumerable<int>> GetLatestUnseenMessagesIds(int userId1, int userId2);
        Task<IEnumerable<Message>> FilterMessagesAsync(int authUserId, 
            int userId, 
            string? searchQuery, 
            DateTime? dateTime, 
            int pageNo, 
            int pageSize,
            string[] includes = null, 
            Expression<Func<Message, object>> sortingExpression = null, 
            OrderBy sortingDirection = OrderBy.Ascending);
    }
}
