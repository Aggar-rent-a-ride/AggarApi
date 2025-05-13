using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs;

namespace CORE.Services.IServices
{
    public interface INotificationService
    {
        Task<ResponseDto<object>> Acknowledge(HashSet<int> notificationIds, int userId); 
    }
}
