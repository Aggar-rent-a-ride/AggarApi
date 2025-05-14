using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs;
using CORE.DTOs.Notification;

namespace CORE.Services.IServices
{
    public interface INotificationService
    {
        Task<ResponseDto<object>> AcknowledgeAsync(HashSet<int> notificationIds, int userId); 
        Task<ResponseDto<PagedResultDto<IEnumerable<GetNotificationDto>>>> GetNotificationsAsync(int userId, int pageNo, int pageSize, int maxPageSize = 100);
    }
}
