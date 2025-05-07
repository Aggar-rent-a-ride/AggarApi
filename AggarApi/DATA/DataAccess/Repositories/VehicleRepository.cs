using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.Models;
using DATA.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Repositories
{
    public class VehicleRepository : BaseRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<Vehicle> GetVehicles(int? brandId, int? typeId, VehicleTransmission? transmission, string? searchKey, decimal? minPrice, decimal? maxPrice, int? year, double? rate)
        {
            IQueryable<Vehicle> vehicles = _context.Vehicles
                .Include(v => v.VehicleBrand)
                .Include(v => v.VehicleType)
                .AsNoTracking()
                .AsQueryable();

            vehicles = vehicles.Where(v => v.Status == VehicleStatus.Active);

            if (brandId.HasValue)
                vehicles = vehicles.Where(v => v.VehicleBrandId == brandId);

            if (typeId.HasValue)
                vehicles = vehicles.Where(v => v.VehicleTypeId == typeId);

            if (transmission.HasValue)
                vehicles = vehicles.Where(v => v.Transmission == transmission);

            if (minPrice.HasValue)
                vehicles = vehicles.Where(v => v.PricePerDay >= minPrice);

            if (maxPrice.HasValue)
                vehicles = vehicles.Where(v => v.PricePerDay <= maxPrice);

            if (rate.HasValue)
                vehicles = vehicles.Where(v => v.Rate >= rate);

            if (year.HasValue)
                vehicles = vehicles.Where(v => v.Year >= year);

            if (searchKey is not null)
                vehicles = vehicles.Where(v =>
                    (v.Requirements != null && v.Requirements.ToLower().Contains(searchKey.ToLower())) ||
                    (v.ExtraDetails != null && v.ExtraDetails.ToLower().Contains(searchKey.ToLower())) ||
                    (v.Model != null && v.Model.ToLower().Contains(searchKey.ToLower())) ||
                    (v.Color != null && v.Color.ToLower().Contains(searchKey.ToLower())));

            return vehicles;
        }
    
        public async Task<Vehicle?> GetVehicleByRentalIdAsync(int rentalId)
        {
            var rental = await _context.Rentals
                .Include(r => r.Booking)
                .ThenInclude(b => b.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            return rental?.Booking?.Vehicle;
        }
    }
}
