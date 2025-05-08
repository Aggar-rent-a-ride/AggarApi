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
        Task<ResponseDto<PagedResultDto<GetVehicleSummaryDto>>> GetVehiclesAsync(int userId, VehiclesSearchQuery searchQuery);
        Task<ResponseDto<object>> DeleteVehicleByIdAsync(int vehicleId, int? renterId);
        Task<ResponseDto<GetVehicleDto>> UpdateVehicleImagesAsync(UpdateVehicleImagesDto updateVehicleImagesDto, int? renterId);
        Task<ResponseDto<GetVehicleDto>> UpdateVehicleAsync(UpdateVehicleDto updateVehicleDto, int? renterId);
        Task<ResponseDto<GetVehicleDto>> UpdateVehicleDiscountsAsync(UpdateVehicleDiscountsDto updateVehicleDiscountsDto, int? renterId);
        Task<ResponseDto<GetVehicleDto>> GetVehicleByRentalIdAsync(int rentalId);
        Task<bool> CheckVehicleAvailability(Vehicle vehicle, DateTime startDate, DateTime endDate);
    }
}
