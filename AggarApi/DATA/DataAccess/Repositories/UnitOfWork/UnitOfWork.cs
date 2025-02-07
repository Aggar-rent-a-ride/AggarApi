using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IBaseRepository<AppUser> AppUsers { get; private set; }
        public IVehicleRepository Vehicles { get; private set; }
        public IBaseRepository<Discount> Discounts { get; private set; }
        public IBaseRepository<Booking> Bookings { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            AppUsers = new BaseRepository<AppUser>(_context);
            Vehicles = new VehicleRepository(_context);
        }

        public async Task<int> CommitAsync() => await _context.SaveChangesAsync();

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
