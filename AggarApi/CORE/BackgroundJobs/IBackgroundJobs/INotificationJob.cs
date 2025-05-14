using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs.Notification;

namespace CORE.BackgroundJobs.IBackgroundJobs
{
    public interface INotificationJob
    {
        Task ExecuteAsync(CreateNotificationDto dto);
    }
}
