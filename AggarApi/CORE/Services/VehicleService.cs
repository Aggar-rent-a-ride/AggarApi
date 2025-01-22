using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using DATA.Models.Enums;
﻿using CORE.DTOs;
using CORE.DTOs.Vehicle;
using System;
using CORE.Constants;
using CORE.Extensions;
using Microsoft.EntityFrameworkCore;
using CORE.Helpers;

namespace CORE.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<ResponseDto<GetVehicleDto>> CreateVehicleAsync(CreateVehicleDto createVehicleDto)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDto<PagedResultDto<GetVehicleSummaryDto>>> GetNearestVehiclesAsync(int userId, int pageNo, int pageSize, string? searchKey, int? brandId, int? typeId, VehicleTransmission? transmission, double? Rate, double? minPrice, double? maxPrice, int? year)
        {
            AppUser? user = await _unitOfWork.AppUsers.GetAsync(userId);
            if (user == null)
                return new ResponseDto<PagedResultDto<GetVehicleSummaryDto>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Authentication Failed"
                };

            Location userLocation = user.Location;

            pageNo = pageNo <= 0 ? 1 : pageNo;

            IQueryable<Vehicle> vehicles = _unitOfWork.Vehicles.GetNearestVehicles(brandId, typeId, transmission, searchKey, minPrice, maxPrice, year, Rate);

            List<GetVehicleSummaryDto> data = await vehicles
                .Select(v => new GetVehicleSummaryDto
            {
                Id = v.Id,
                Brand = v.VehicleBrand != null ? v.VehicleBrand.Name : null,
                Type = v.VehicleType != null ? v.VehicleType.Name : null,
                Model = v.Model,
                Year = v.Year,
                PricePerHour = v.PricePerHour,
                Rate = v.Rate,
                Distance = userLocation.CalculateDistance(v.Location),
            })
                .OrderBy(dto => dto.Distance)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedData = new PagedResultDto<GetVehicleSummaryDto>
            {
                Data = data,
                PageNumber = pageNo,
                PageSize = pageSize,
                TotalPages = PaginationHelper.CalculateTotalPages(data.Count, pageSize)
            };

            return new ResponseDto<PagedResultDto<GetVehicleSummaryDto>>
            {
                Data = pagedData,
                Message = "Vehicles Loaded Successfully...",
                StatusCode = StatusCodes.OK,
            };
        }
    }
}
