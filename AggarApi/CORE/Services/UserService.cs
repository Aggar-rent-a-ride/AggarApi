using AutoMapper;
using CORE.BackgroundJobs;
using CORE.BackgroundJobs.IBackgroundJobs;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.AppUser;
using CORE.DTOs.Auth;
using CORE.DTOs.Paths;
using CORE.DTOs.Review;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Constants;
using DATA.Constants.Enums;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using DATA.Models.Enums;
using Hangfire;
using Microsoft.AspNetCore.Identity;
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
        private readonly IUserManagementJob _userManagementJob;
        private readonly IEmailTemplateRendererService _emailTemplateRendererService;
        private readonly IOptions<WarningManagement> _warningManagement;
        private readonly IEmailSendingJob _emailSendingJob;
        private readonly UserManager<AppUser> _userManager;
        private readonly IFileService _fileService;
        private readonly Paths _paths;
        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IMapper mapper, IUserManagementJob userManagementJob, IEmailService emailService, IEmailTemplateRendererService emailTemplateRendererService, IOptions<WarningManagement> warningManagement, IEmailSendingJob emailSendingJob, UserManager<AppUser> userManager, IFileService fileService, IOptions<Paths> paths)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _userManagementJob = userManagementJob;
            _emailTemplateRendererService = emailTemplateRendererService;
            _warningManagement = warningManagement;
            _emailSendingJob = emailSendingJob;
            _userManager = userManager;
            _fileService = fileService;
            _paths = paths.Value;
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
            if (userId != authUserId && roles.Contains(Roles.Admin) == false)
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
            if (changes == 0)
            {
                _logger.LogInformation("Failed to delete user with ID {UserId}.", userId);
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to delete user."
                };
            }

            _emailSendingJob.SendEmail(user.Email, EmailSubject.AccountRemoved, await _emailTemplateRendererService.RenderTemplateAsync(Templates.Notification, new { NotificationContent = System.Web.HttpUtility.HtmlEncode("Your account has been removed."), NotificationType = System.Web.HttpUtility.HtmlEncode(NotificationType.AccountRemoved) }));

            _logger.LogInformation("User with ID {UserId} deleted successfully.", userId);
            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.OK,
                Message = "User deleted successfully."
            };
        }
        public async Task<ResponseDto<PagedResultDto<IEnumerable<SummerizedUserWithRateDto>>>> FindUsersAsync(string? searchKey, int pageNo, int pageSize, int maxPageSize = 100)
        {
            if (PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string paginationError)
            {
                _logger.LogWarning("Invalid pagination parameters: {ErrorMessage}", paginationError);
                return new ResponseDto<PagedResultDto<IEnumerable<SummerizedUserWithRateDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = paginationError
                };
            }
            var users = new List<AppUser>();
            var count = 0;
            if (string.IsNullOrWhiteSpace(searchKey) == true)
            {
                users = (await _unitOfWork.AppUsers.GetAllAsync(pageNo, pageSize)).ToList();
                count = await _unitOfWork.AppUsers.CountAsync();
            }
            else
            {
                users = (await _unitOfWork.AppUsers.FindAsync(u => u.UserName.Contains(searchKey) || u.Name.Contains(searchKey), pageNo, pageSize)).ToList();
                count = await _unitOfWork.AppUsers.CountAsync(u => u.UserName.Contains(searchKey) || u.Name.Contains(searchKey));
            }
            return new ResponseDto<PagedResultDto<IEnumerable<SummerizedUserWithRateDto>>>
            {
                Data = PaginationHelpers.CreatePagedResult(_mapper.Map<IEnumerable<SummerizedUserWithRateDto>>(users), pageNo, pageSize, count),
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
            if (banDurationInDays <= 0)
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
            var refreshTokens = user.RefreshTokens.Where(r => r.IsActive);
            foreach (var refreshToken in refreshTokens)
                refreshToken.RevokedOn = DateTime.UtcNow;

            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
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
            if (banDurationInDays <= 3 * 365)
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
            if (user.WarningCount == _warningManagement.Value.MaxWarningsCount - 1 && user.TotalWarningsCount == _warningManagement.Value.MaxTotalWarningsCount - 1)
            {
                _logger.LogInformation("User with ID {UserId} has reached the maximum warning limit and is going to be banned forever", user.Id);
                return await BanUserAsync(user, 1000000);
            }
            else if (user.WarningCount == _warningManagement.Value.MaxWarningsCount - 1)
            {
                user.WarningCount = 0;
                user.TotalWarningsCount++;
                isTotalWarningsIncreased = true;
            }
            else
                user.WarningCount++;

            var changes = await _unitOfWork.CommitAsync();
            if (changes == 0)
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
        public async Task<ResponseDto<PagedResultDto<IEnumerable<SummerizedUserDto>>>> GetTotalUsersAsync(string? role, int pageNo, int pageSize, DateRangePreset? dateFilter, int maxPageSize = 100)
        {
            if (PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string paginationError)
            {
                _logger.LogWarning("Invalid pagination parameters: {ErrorMessage}", paginationError);
                return new ResponseDto<PagedResultDto<IEnumerable<SummerizedUserDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = paginationError
                };
            }

            var tupleResult = await _unitOfWork.AppUsers.GetTotalUsersAsync(role, pageNo, pageSize, dateFilter);
            var result = _mapper.Map<IEnumerable<SummerizedUserDto>>(tupleResult.appUsers);

            return new ResponseDto<PagedResultDto<IEnumerable<SummerizedUserDto>>>
            {
                Data = PaginationHelpers.CreatePagedResult(result, pageNo, pageSize, tupleResult.Count),
                StatusCode = StatusCodes.OK,
            };
        }
        public async Task<ResponseDto<int>> GetTotalUsersCountAsync(string? role)
        {
            return new ResponseDto<int>
            {
                Data = await _unitOfWork.AppUsers.GetTotalUsersCountAsync(role),
                StatusCode = StatusCodes.OK,
            };
        }

        public async Task<ResponseDto<SummerizedUserDto>> GetUserByIdAsync(int userId)
        {
            var user = await _unitOfWork.AppUsers.GetAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return new ResponseDto<SummerizedUserDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "User not found."
                };
            }
            return new ResponseDto<SummerizedUserDto>
            {
                Data = _mapper.Map<SummerizedUserDto>(user),
                StatusCode = StatusCodes.OK,
            };
        }

        public async Task<ResponseDto<UserProfileDto>> GetUserProfileAsync(int userId)
        {
            AppUser? user = await _unitOfWork.AppUsers.GetAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return new ResponseDto<UserProfileDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "User not found."
                };
            }

            var profiledto = _mapper.Map<UserProfileDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            profiledto.Role = roles.Where(r => r != Roles.User).First();

            return new ResponseDto<UserProfileDto>
            {
                Data = profiledto,
                StatusCode = StatusCodes.OK,
                Message = "Profile Loaded Successfuly."
            };
        }

        public async Task<ResponseDto<UserProfileDto>> UpdateUserProfileAsync(int userId, UpdateProfileDto dto)
        {
            AppUser? user = await _unitOfWork.AppUsers.GetAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return new ResponseDto<UserProfileDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "User not found."
                };
            }

            _mapper.Map(dto, user);
            if(dto.Image != null)
            {
                _logger.LogInformation($"Update image profile for user {userId}", userId);
                user.ImagePath = await _fileService.UploadFileAsync(_paths.Profiles, user.ImagePath, dto.Image, AllowedExtensions.ImageExtensions);
            }

            await _unitOfWork.AppUsers.AddOrUpdateAsync(user);
            int changes = await _unitOfWork.CommitAsync();

            if(changes == 0)
            {
                _logger.LogWarning($"Failed to update profile for user {userId}", userId);
                return new ResponseDto<UserProfileDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Failed to update user profile"
                };
            }

            var result = await GetUserProfileAsync(userId);

            return new ResponseDto<UserProfileDto>
            {
                Data = result.Data,
                StatusCode = StatusCodes.OK,
                Message = "Profile Updated Successfuly."
            };
        }

        public async Task<ResponseDto<object>> RemoveProfileImageAsync(int userId)
        {
            AppUser? user = await _unitOfWork.AppUsers.GetAsync(userId);

            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found.", userId);
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "User not found."
                };
            }

            if (user.ImagePath == null)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "There is no profile image to remove."
                };
            }

            bool result = _fileService.DeleteFile(user.ImagePath);

            if (!result)
            {
                _logger.LogError($"Failed to remove profile image for user {user.Id}", user.Id);
                return new ResponseDto<object>
                {
                    Message = "Failed To Remove Profile Image.",
                    StatusCode = StatusCodes.InternalServerError
                };
            }

            user.ImagePath = null;

            await _unitOfWork.AppUsers.AddOrUpdateAsync(user);
            int changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
            {
                _logger.LogError($"Failed to update user {user.Id}", user.Id);
                return new ResponseDto<object>
                {
                    Message = "Profile image removed successfuly but failed to update user.",
                    StatusCode = StatusCodes.InternalServerError
                };
            }

            _logger.LogInformation($"Removed profile image for user {user.Id}", user.Id);

            return new ResponseDto<object>
            {
                Message = "Profile Image Removed Successfuly.",
                StatusCode = StatusCodes.OK,
            };

        }
    }
}
