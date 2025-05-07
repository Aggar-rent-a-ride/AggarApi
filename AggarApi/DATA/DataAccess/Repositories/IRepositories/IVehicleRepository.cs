using DATA.Models;
using DATA.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Repositories.IRepositories
{
    public interface IVehicleRepository : IBaseRepository<Vehicle>
    {
        public IQueryable<Vehicle> GetVehicles(int? brandId, int? typeId, VehicleTransmission? transmission, string? searchKey, decimal? minPrice, decimal? maxPrice, int? year, double? rate);
        Task<Vehicle?> GetVehicleByRentalIdAsync(int rentalId);
    }
}
