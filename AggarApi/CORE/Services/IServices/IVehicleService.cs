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
        Task<ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>> GetVehiclesAsync(int userId, VehiclesSearchQuery searchQuery);
        Task<ResponseDto<object>> DeleteVehicleByIdAsync(int vehicleId, int? renterId, string[] roles);
        Task<ResponseDto<GetVehicleDto>> UpdateVehicleImagesAsync(UpdateVehicleImagesDto updateVehicleImagesDto, int? renterId);
        Task<ResponseDto<GetVehicleDto>> UpdateVehicleAsync(UpdateVehicleDto updateVehicleDto, int? renterId);
        Task<ResponseDto<GetVehicleDto>> UpdateVehicleDiscountsAsync(UpdateVehicleDiscountsDto updateVehicleDiscountsDto, int? renterId);
        Task<ResponseDto<GetVehicleDto>> GetVehicleByRentalIdAsync(int rentalId);
        Task<ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>> GetVehiclesByStatusAsync(VehicleStatus status, int pageNo, int pageSize, int maxPageSize = 100);
        Task<ResponseDto<int>> GetVehiclesByStatusCountAsync(VehicleStatus status);
        Task<ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>> GetMostRentedVehiclesAsync(int pageNo, int pageSize, int maxPageSize = 50);
        Task<ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>> GetPopularVehiclesAsync(int pageNo, int pageSize, int maxPageSize = 50);
        Task<ResponseDto<object>> VehicleFavouriteAsync(int customerId, SetVehicleFavouriteDto dto);
    }
}
