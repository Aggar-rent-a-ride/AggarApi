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
        public Task<Vehicle> GetNearestVehicles(int userId);
        public Task<Vehicle> GetNearestVehiclesByCriteria(int userId,
            string? searchKey,
            int? typeId, int? brandId,
            PriceCategory? priceCategory, double? minPrice, double? maxPrice,
            string? model, int? year, double? Rate);
        Task<ResponseDto<GetVehicleDto>> CreateVehicleAsync(CreateVehicleDto createVehicleDto, int? renterId);
        Task<ResponseDto<GetVehicleDto>> GetVehicleByIdAsync(int vehicleId);
        public Task<ResponseDto<PagedResultDto<GetVehicleSummaryDto>>> GetNearestVehiclesAsync(int userId, int pageNo, int pageSize, 
            string? searchKey, int? brandId, int? typeId, VehicleTransmission? transmission, 
            double? Rate, double? minPrice, double? maxPrice, int? year);
        Task<ResponseDto<GetVehicleDto>> CreateVehicleAsync(CreateVehicleDto createVehicleDto);
    }
}
