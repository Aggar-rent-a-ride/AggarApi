using AutoMapper;
using CORE.BackgroundJobs;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.AppUser;
using CORE.DTOs.Auth;
using CORE.DTOs.Review;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Constants;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using DATA.Models.Enums;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly UserManagementJob _userManagementJob;
        private readonly IEmailTemplateRendererService _emailTemplateRendererService;
        private readonly IOptions<WarningManagement> _warningManagement;
        private readonly EmailSendingJob _emailSendingJob;
        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IMapper mapper, UserManagementJob userManagementJob, IEmailService emailService, IEmailTemplateRendererService emailTemplateRendererService, IOptions<WarningManagement> warningManagement, EmailSendingJob emailSendingJob)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _userManagementJob = userManagementJob;
            _emailTemplateRendererService = emailTemplateRendererService;
            _warningManagement = warningManagement;
            _emailSendingJob = emailSendingJob;
        }

        public async Task<bool> CheckAllUsersExist(List<int> userIds)
        {
            var count = await _unitOfWork.AppUsers.CountAsync(u => userIds.Contains(u.Id));
            
            return count == userIds.Count;
        }

        public async Task<bool> CheckAnyAsync(int userId)
        {
            return await _unitOfWork.AppUsers.CheckAnyAsync(x => x.Id == userId, null);
        }

        public async Task<ResponseDto<object>> DeleteUserAsync(int userId, int authUserId, string[] roles)
        {
            if(userId != authUserId && roles.Contains(Roles.Admin) == false)
            {
                _logger.LogInformation("User {UserId} tried to delete user {TargetUserId} without permission.", authUserId, userId);
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.Forbidden,
                    Message = "This is not your account to delete."
                };
            }
            var user = await _unitOfWork.AppUsers.GetAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "User not found."
                };
            }
            _unitOfWork.AppUsers.Delete(user);
            var changes = await _unitOfWork.CommitAsync();
            if(changes == 0)
            {
                _logger.LogInformation("Failed to delete user with ID {UserId}.", userId);
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to delete user."
                };
            }

            _logger.LogInformation("User with ID {UserId} deleted successfully.", userId);
            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.OK,
                Message = "User deleted successfully."
            };
        }

        public async Task<ResponseDto<IEnumerable<SummerizedUserWithRateDto>>> FindUsersAsync(string? searchKey, int pageNo, int pageSize, int maxPageSize = 100)
        {
            if (PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string paginationError)
            {
                _logger.LogWarning("Invalid pagination parameters: {ErrorMessage}", paginationError);
                return new ResponseDto<IEnumerable<SummerizedUserWithRateDto>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = paginationError
                };
            }
            var users = new List<AppUser>();
            if (string.IsNullOrWhiteSpace(searchKey) == true)
                users = (await _unitOfWork.AppUsers.GetAllAsync(pageNo, pageSize)).ToList();
            else 
                users = (await _unitOfWork.AppUsers.FindAsync(u => u.UserName.Contains(searchKey) || u.Name.Contains(searchKey), pageNo, pageSize)).ToList();
            return new ResponseDto<IEnumerable<SummerizedUserWithRateDto>>()
            {
                Data = _mapper.Map<IEnumerable<SummerizedUserWithRateDto>>(users),
                StatusCode = StatusCodes.OK,
            };
        }
        private async Task<ResponseDto<object>> BanUserAsync(AppUser user, int? banDurationInDays)
        {
            if (banDurationInDays == null)
            {
                _logger.LogWarning("banDurationInDays is required for banning a user.");
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "banDurationInDays is required."
                };
            }
            if(banDurationInDays <= 0)
            {
                _logger.LogWarning("banDurationInDays must be positive for banning a user.");
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "banDurationInDays must be positive."
                };
            }

            var bannedTo = DateTime.UtcNow.AddDays((double)banDurationInDays);

            user.BannedTo = bannedTo;
            user.Status = UserStatus.Banned;
            
            var changes = await _unitOfWork.CommitAsync();

            if(changes == 0)
            {
                _logger.LogWarning("Failed to ban user with ID {UserId}.", user.Id);
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to ban user."
                };
            }

            _logger.LogInformation("User with ID {UserId} banned until {BannedTo}.", user.Id, bannedTo);

            // Schedule unban job
            if(banDurationInDays <= 3*365)
                await _userManagementJob.ScheduleUserUnbanAsync(user.Id, bannedTo);

            //send email to the user
            _emailSendingJob.SendEmail(user.Email, EmailSubject.AccountBanned, await _emailTemplateRendererService.RenderTemplateAsync(Templates.Ban, new { Name = System.Web.HttpUtility.HtmlEncode(user.Name), BannedTo = System.Web.HttpUtility.HtmlEncode(user.BannedTo.Value.ToString("MMMM dd, yyyy")) }));

            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.OK,
                Message = "User banned successfully."
            };
        }
        private async Task<ResponseDto<object>> WarnUserAsync(AppUser user)
        {
            bool isTotalWarningsIncreased = false;
            if(user.WarningCount == _warningManagement.Value.MaxWarningsCount - 1 && user.TotalWarningsCount == _warningManagement.Value.MaxTotalWarningsCount - 1)
            {
                _logger.LogInformation("User with ID {UserId} has reached the maximum warning limit and is going to be banned forever", user.Id);
                return await BanUserAsync(user, 1000000);
            }
            else if(user.WarningCount == _warningManagement.Value.MaxWarningsCount - 1)
            {
                user.WarningCount = 0;
                user.TotalWarningsCount++;
                isTotalWarningsIncreased = true;
            }
            else
                user.WarningCount++;

            var changes = await _unitOfWork.CommitAsync();
            if(changes == 0)
            {
                _logger.LogWarning("Failed to warn user with ID {UserId}.", user.Id);
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to warn user."
                };
            }

            _logger.LogInformation("User with ID {UserId} warned successfully. Total warnings: {TotalWarnings}, Current warnings: {CurrentWarnings}", user.Id, user.TotalWarningsCount, user.WarningCount);

            if (isTotalWarningsIncreased == true)
                _emailSendingJob.SendEmail(user.Email, EmailSubject.AccountWarned, await _emailTemplateRendererService.RenderTemplateAsync(Templates.Warning, new { Name = System.Web.HttpUtility.HtmlEncode(user.Name), WarningsLeft = System.Web.HttpUtility.HtmlEncode(_warningManagement.Value.MaxTotalWarningsCount - user.TotalWarningsCount) }));
            
            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.OK,
                Message = "User warned successfully."
            };
        }
        public async Task<ResponseDto<object>> PunishUserAsync(PunishUserDto dto)
        {
            var user = await _unitOfWork.AppUsers.GetAsync(dto.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", dto.UserId);
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "User not found."
                };
            }

            if (dto.Type == DTOs.AppUser.Enums.PunishmentType.Ban)
                return await BanUserAsync(user, dto.BanDurationInDays);

            return await WarnUserAsync(user);
        }
    }
}
