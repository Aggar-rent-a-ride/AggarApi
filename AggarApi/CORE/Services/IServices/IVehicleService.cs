using DATA.Models;
using DATA.Models.Enums;
﻿using CORE.DTOs;
using CORE.DTOs.Vehicle;
using CORE.DTOs.Discount;

namespace CORE.Services.IServices
{
    public interface IVehicleService
    {
        Task<ResponseDto<GetVehicleDto>> CreateVehicleAsync(CreateVehicleDto createVehicleDto, int? renterId);
        Task<ResponseDto<GetVehicleDto>> GetVehicleByIdAsync(int vehicleId);
        Task<ResponseDto<PagedResultDto<GetVehicleSummaryDto>>> GetNearestVehiclesAsync(int userId, int pageNo, int pageSize, 
            string? searchKey, int? brandId, int? typeId, VehicleTransmission? transmission, 
            double? Rate, decimal? minPrice, decimal? maxPrice, int? year, Location? location = null);
        Task<ResponseDto<object>> DeleteVehicleByIdAsync(int vehicleId, int? renterId);
        Task<ResponseDto<GetVehicleDto>> UpdateVehicleImagesAsync(UpdateVehicleImagesDto updateVehicleImagesDto, int? renterId);
        Task<ResponseDto<GetVehicleDto>> UpdateVehicleAsync(UpdateVehicleDto updateVehicleDto, int? renterId);
        Task<ResponseDto<GetVehicleDto>> UpdateVehicleDiscountsAsync(UpdateVehicleDiscountsDto updateVehicleDiscountsDto, int? renterId);
    }
}
