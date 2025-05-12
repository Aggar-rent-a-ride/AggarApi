using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.Models;
using Microsoft.EntityFrameworkCore.Storage;
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
        private IDbContextTransaction _transaction;
        private readonly ILogger<UnitOfWork> _logger;

        public IUserRepository AppUsers { get; private set; }
        public IVehicleRepository Vehicles { get; private set; }
        public IBaseRepository<Discount> Discounts { get; private set; }
        public IBookingRepository Bookings { get; private set; }
        public IBaseRepository<FileCache> FileCache { get; private set; }
        public IBaseRepository<Renter> Renters { get; private set; }
        public IChatRepository Chat { get; private set; }
        public IBaseRepository<VehicleBrand> VehicleBrands { get; private set; }
        public IBaseRepository<VehicleType> VehicleTypes { get; private set; }
        public IRentalRepository Rentals { get; private set; }
        public IRenterReviewRepository RenterReviews { get; private set; }
        public ICustomerReviewRepository CustomerReviews { get; private set; }
        public IReportRepository Reports { get; private set; }

        public IBaseRepository<VehiclePopularity> VehiclePopularity { get; private set; }

        public UnitOfWork(AppDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger;
            AppUsers = new UserRepository(_context);
            Vehicles = new VehicleRepository(_context);
            Discounts = new BaseRepository<Discount>(_context);
            Bookings = new BookingRepository(_context);
            Renters = new BaseRepository<Renter>(_context);
            FileCache = new BaseRepository<FileCache>(_context);
            VehicleBrands = new BaseRepository<VehicleBrand>(_context);
            VehicleTypes = new BaseRepository<VehicleType>(_context);
            Chat = new ChatRepository(_context);
            Rentals = new RentalRepository(_context);
            RenterReviews = new RenterReviewRepository(_context);
            CustomerReviews = new CustomerReviewRepository(_context);
            Reports = new ReportRepository(_context);
            VehiclePopularity = new BaseRepository<VehiclePopularity>(_context);
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

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                await RollbackTransactionAsync();
                throw;
            }
        }
        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during transaction rollback: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
