using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CORE.DTOs.Notification;
using DATA.Models;

namespace CORE.AutoMapperProfiles
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, GetNotificationDto>();
        }
    }
}
