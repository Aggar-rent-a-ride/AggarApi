using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using DATA.Models.Enums;
﻿using CORE.DTOs;
using CORE.DTOs.Vehicle;
using CORE.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<Vehicle> GetNearestVehicles(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<Vehicle> GetNearestVehiclesByCriteria(int userId, string? searchKey, int? typeId, int? brandId, PriceCategory? priceCategory, double? minPrice, double? maxPrice, string? model, int? year, double? Rate)
        {
            throw new NotImplementedException();
        }

        public Task<Vehicle> GetVehicles()
        public Task<ResponseDto<GetVehicleDto>> CreateVehicleAsync(CreateVehicleDto createVehicleDto)
        {
            throw new NotImplementedException();
        }
    }
}
