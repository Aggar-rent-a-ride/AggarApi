using CORE.DTOs;
using CORE.DTOs.Chat;
using CORE.DTOs.Message;
using DATA.Constants.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IChatService
    {
        Task<ResponseDto<TGet>> CreateMessageAsync<TCreate, TGet>(TCreate messageDto, int senderId)
            where TCreate : CreateMessageDto
            where TGet : GetMessageDto;
        Task<ResponseDto<ArrayList>> GetMessagesAsync(int userId1, int userId2, DateTime dateTime, int pageSize, DateFilter dateFilter, int maxPageSize = 100);
        Task<ResponseDto<ArrayList>> GetChatAsync(int authUserId, int pageNo, int pageSize, int maxPageSize = 100);
        Task<ResponseDto<object>> AcknowledgeMessagesAsync(int authUserId, HashSet<int> messageIds);
        Task<ResponseDto<ArrayList>> FilterMessagesAsync (MessageFilterDto filter, int authUserId, int maxPageSize = 100);
    }
}
