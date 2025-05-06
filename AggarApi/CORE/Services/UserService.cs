using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.AppUser;
using CORE.DTOs.Review;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Hangfire;
using Microsoft.Extensions.Logging;
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
        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
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
    }
}
