﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.Constants;
using DATA.Constants.Enums;
using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Operation.Valid;

namespace DATA.DataAccess.Repositories
{
    public class UserRepository : BaseRepository<AppUser>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<AppUser> appUsers, int Count)> GetTotalUsersAsync(string? role, int pageNo, int pageSize, DateRangePreset? dateFilter)
        {
            var query = _context.Users.AsQueryable();
            if(string.IsNullOrWhiteSpace(role) == false)
            {
                query = query.Where(u =>
                    _context.UserRoles.Any(ur =>
                        ur.UserId == u.Id &&
                        _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == role)
                    )
                );
            }
            if (dateFilter != null)
            {
                var curDate = DateTime.UtcNow;
                switch(dateFilter)
                {
                    case DateRangePreset.Today:
                        query = query.Where(u => u.CreatedAt.Date == curDate.Date);
                        break;
                    case DateRangePreset.Yesterday:
                        query = query.Where(u => u.CreatedAt.Date == curDate.AddDays(-1).Date);
                        break;
                    case DateRangePreset.Last7Days:
                        query = query.Where(u => u.CreatedAt.Date >= curDate.AddDays(-7).Date);
                        break;
                    case DateRangePreset.Last30Days:
                        query = query.Where(u => u.CreatedAt.Date >= curDate.AddDays(-30).Date);
                        break;
                    case DateRangePreset.Last365Days:
                        query = query.Where(u => u.CreatedAt.Date >= curDate.AddDays(-365).Date);
                        break;
                    default:
                        break;
                }
            }
            
            var count = query.Count();
            var appUsers = await query
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (appUsers, count);
        }

        public async Task<int> GetTotalUsersCountAsync(string? role)
        {
            var query = _context.Users.AsQueryable();
            if (string.IsNullOrWhiteSpace(role) == false)
            {
                query = query.Where(u =>
                    _context.UserRoles.Any(ur =>
                        ur.UserId == u.Id &&
                        _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == role)
                    )
                );
            }
            return await query.CountAsync();
        }
    }
}
