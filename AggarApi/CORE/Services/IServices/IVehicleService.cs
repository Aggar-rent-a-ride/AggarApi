using DATA.Models;
using DATA.Models.Enums;
﻿using CORE.DTOs;
using CORE.DTOs.Vehicle;

namespace CORE.Services.IServices
{
    public interface IVehicleService
    {
        Task<ResponseDto<GetVehicleDto>> CreateVehicleAsync(CreateVehicleDto createVehicleDto, int? renterId);
        Task<ResponseDto<GetVehicleDto>> GetVehicleByIdAsync(int vehicleId);
        Task<ResponseDto<PagedResultDto<GetVehicleSummaryDto>>> GetNearestVehiclesAsync(int userId, int pageNo, int pageSize, 
            string? searchKey, int? brandId, int? typeId, VehicleTransmission? transmission, 
            double? Rate, double? minPrice, double? maxPrice, int? year, string baseUrl);
    }
}
