using Azure;
using DATA.Constants;
using DATA.Constants.Enums;
using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Repositories
{
    public class ChatRepository : BaseRepository<Message>, IChatRepository
    {
        public ChatRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Message>> FilterMessagesAsync(int authUserId, 
            int userId, 
            string? searchQuery, 
            DateTime? dateTime, 
            int pageNo, 
            int pageSize, 
            string[] includes = null, 
            Expression<Func<Message, object>> sortingExpression = null, 
            OrderBy sortingDirection = OrderBy.Ascending)
        {
            IQueryable<Message> query = _context.Messages
                .Where(m=>
                (m.SenderId == userId && m.ReceiverId == authUserId) || 
                (m.SenderId == authUserId && m.ReceiverId == userId));

            if(dateTime != null)
                query = query.Where(m=>m.SentAt.Date == dateTime.Value.Date);

            if (string.IsNullOrWhiteSpace(searchQuery) == false)
                query = query.Where(m => (m.MessageType == MessageType.ContentMessage && ((ContentMessage)m).Content.Contains(searchQuery)));


            if (sortingExpression != null)
            {
                if (sortingDirection == OrderBy.Ascending)
                    query = query.OrderBy(sortingExpression);
                else
                    query = query.OrderByDescending(sortingExpression);
            }

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.Skip((pageNo - 1) * pageSize).Take(pageSize)
            .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetLatestChatMessagesAsync(int authUserId, int pageNo, int pageSize)
        {
            // Create a SQL query that will work correctly
            var sql = @"
                    WITH RankedMessages AS (
                        SELECT 
                            M.*,
                            ROW_NUMBER() OVER (
                                PARTITION BY 
                                    CASE 
                                        WHEN M.SenderId = {0} THEN M.ReceiverId 
                                        ELSE M.SenderId 
                                    END
                                ORDER BY M.Id DESC
                            ) AS RowNum
                        FROM Messages M
                        WHERE M.SenderId = {0} OR M.ReceiverId = {0}
                    )
                    SELECT RM.Id
                    FROM RankedMessages RM
                    WHERE RM.RowNum = 1
                    ORDER BY RM.Id DESC
                    OFFSET {1} ROWS
                    FETCH NEXT {2} ROWS ONLY";

            // First get just the IDs of the messages we need
            var messageIds = await _context.Database
                .SqlQueryRaw<int>(sql, authUserId, (pageNo - 1) * pageSize, pageSize)
                .ToHashSetAsync();

            // Then get the full message objects with includes
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => messageIds.Contains(m.Id))
                .OrderByDescending(m => m.Id)
                .ToListAsync();


            return messages;
        }

        public async Task<IEnumerable<int>> GetLatestUnseenMessagesIds(int userId1, int userId2)
        {
            return await _context.Messages
                .Where(m => m.SenderId == userId2 && m.ReceiverId == userId1 && m.IsSeen == false)
                .Select(m => m.Id)
                .ToListAsync();
        }
    }
}
