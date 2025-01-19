using DATA.Models;
using DATA.Models.Enums;
﻿using CORE.DTOs;
using CORE.DTOs.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IVehicleService
    {
        public Task<Vehicle> GetVehicles();
        public Task<Vehicle> GetNearestVehicles(int userId);
        public Task<Vehicle> GetNearestVehiclesByCriteria(int userId,
            string? searchKey,
            int? typeId, int? brandId,
            PriceCategory? priceCategory, double? minPrice, double? maxPrice,
            string? model, int? year, double? Rate);
        Task<ResponseDto<GetVehicleDto>> CreateVehicleAsync(CreateVehicleDto createVehicleDto);
    }
}
