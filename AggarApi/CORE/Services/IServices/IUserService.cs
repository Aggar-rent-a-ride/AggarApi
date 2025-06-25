using CORE.DTOs;
using CORE.DTOs.AppUser;
using DATA.Constants.Enums;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IUserService
    {
        Task<bool> CheckAnyAsync(int userId);
        Task<bool> CheckAllUsersExist(List<int> userIds);
        Task<ResponseDto<PagedResultDto<IEnumerable<SummerizedUserWithRateDto>>>> FindUsersAsync(string? searchKey, int pageNo, int pageSize, int maxPageSize = 100);
        Task<ResponseDto<object>> DeleteUserAsync(int userId, int authUserId, string[] roles);
        Task<ResponseDto<object>> PunishUserAsync(PunishUserDto dto);
        Task<ResponseDto<PagedResultDto<IEnumerable<SummerizedUserDto>>>> GetTotalUsersAsync(string? role, int pageNo, int pageSize, DateRangePreset? dateFilter, int maxPageSize = 100);
        Task<ResponseDto<int>> GetTotalUsersCountAsync(string? role);
        Task<ResponseDto<SummerizedUserDto>> GetUserByIdAsync(int userId);
        Task<ResponseDto<UserProfileDto>> GetUserProfileAsync(int userId);
        Task<ResponseDto<UserProfileDto>> UpdateUserProfileAsync(int userId, UpdateProfileDto dto);
        Task<ResponseDto<object>> RemoveProfileImageAsync(int userId);
    }
}
