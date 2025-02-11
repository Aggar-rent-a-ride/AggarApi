using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
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

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    }
}
