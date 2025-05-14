using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs.Notification;

namespace CORE.Services.IServices
{
    public interface ISignalRNotificationService
    {
        Task SendNotificationAsync(GetNotificationDto dto, int receiverId);
    }
}
