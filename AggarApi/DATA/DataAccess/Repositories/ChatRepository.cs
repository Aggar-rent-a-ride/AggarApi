using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Repositories
{
    public class ChatRepository : BaseRepository<Message>, IChatRepository
    {
        public ChatRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Message>> GetChatAsync(int authUserId, int pageNo, int pageSize)
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
