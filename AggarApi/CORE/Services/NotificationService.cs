using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Notification;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace CORE.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationService> _logger;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ResponseDto<object>> AcknowledgeAsync(HashSet<int> notificationIds, int userId)
        {
            var notifications = await _unitOfWork.Notifications.GetAllAsync(n => n.ReceiverId == userId && n.IsSeen == false && notificationIds.Contains(n.Id));

            if(notifications.Any() == false)
                _logger.LogWarning("No notifications found to acknowledge");

            foreach (var notification in notifications)
                notification.IsSeen = true;

            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
                _logger.LogWarning("Failed to acknowledge notifications");


            _logger.LogInformation("Notifications acknowledged successfully");
            return new ResponseDto<object>
            {
                StatusCode = 200,
                Message = "Notifications acknowledged successfully",
            };
        }

        public async Task<ResponseDto<PagedResultDto<IEnumerable<GetNotificationDto>>>> GetNotificationsAsync(int userId, int pageNo, int pageSize, int maxPageSize = 100)
        {
            if(PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string paginationError)
            {
                _logger.LogWarning(paginationError);
                return new ResponseDto<PagedResultDto<IEnumerable<GetNotificationDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = paginationError,
                };
            }

            var notifications = await _unitOfWork.Notifications.FindAsync(n => n.ReceiverId == userId,
                pageNo,
                pageSize,
                sortingExpression: n => n.SentAt,
                sortingDirection: DATA.Constants.Enums.OrderBy.Descending);
            var count = await _unitOfWork.Notifications.CountAsync(n => n.ReceiverId == userId);

            return new ResponseDto<PagedResultDto<IEnumerable<GetNotificationDto>>>
            {
                StatusCode = StatusCodes.OK,
                Data = PaginationHelpers.CreatePagedResult(_mapper.Map<IEnumerable<GetNotificationDto>>(notifications), pageNo, pageSize, count)
            };
        }
    }
}
