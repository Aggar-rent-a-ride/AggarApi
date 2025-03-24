using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.Models;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UnitOfWork> _logger;

        public IBaseRepository<AppUser> AppUsers { get; private set; }
        public IVehicleRepository Vehicles { get; private set; }
        public IBaseRepository<Discount> Discounts { get; private set; }
        public IBaseRepository<Booking> Bookings { get; private set; }
        public IBaseRepository<FileCache> FileCache { get; private set; }
        public IBaseRepository<Renter> Renters { get; private set; }
        public IChatRepository Chat { get; private set; }
        public IBaseRepository<VehicleBrand> VehicleBrands { get; private set; }
        public IBaseRepository<VehicleType> VehicleTypes { get; private set; }


        public UnitOfWork(AppDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger;
            AppUsers = new BaseRepository<AppUser>(_context);
            Vehicles = new VehicleRepository(_context);
            Discounts = new BaseRepository<Discount>(_context);
            Bookings = new BaseRepository<Booking>(_context);
            Renters = new BaseRepository<Renter>(_context);
            FileCache = new BaseRepository<FileCache>(_context);
            VehicleBrands = new BaseRepository<VehicleBrand>(_context);
            VehicleTypes = new BaseRepository<VehicleType>(_context);
            Chat = new ChatRepository(_context);
        }

        public async Task<int> CommitAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
