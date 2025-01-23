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
        public Task<ResponseDto<PagedResultDto<GetVehicleSummaryDto>>> GetNearestVehiclesAsync(int userId, int pageNo, int pageSize, 
            string? searchKey, int? brandId, int? typeId, VehicleTransmission? transmission, 
            double? Rate, double? minPrice, double? maxPrice, int? year);
        Task<ResponseDto<GetVehicleDto>> CreateVehicleAsync(CreateVehicleDto createVehicleDto);
    }
}
