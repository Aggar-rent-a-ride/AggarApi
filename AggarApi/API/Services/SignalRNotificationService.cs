using API.Hubs;
using CORE.DTOs.Notification;
using CORE.Services.IServices;
using DATA.Constants;
using Microsoft.AspNetCore.SignalR;

namespace API.Services
{
    public class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(GetNotificationDto dto, int receiverId)
        {
            await _hubContext.Clients.Group(receiverId.ToString()).SendAsync(SignalRMethods.ReceiveNotification, dto);
        }
    }
}
