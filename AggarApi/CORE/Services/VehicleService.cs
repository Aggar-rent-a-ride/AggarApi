using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using DATA.Models.Enums;
﻿using CORE.DTOs;
using CORE.DTOs.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs.Paths;
using Microsoft.Extensions.Options;
using DATA.Constants;
using AutoMapper;
using CORE.Constants;
using CORE.Constants;
using CORE.Extensions;
using Microsoft.EntityFrameworkCore;
using CORE.Helpers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DATA.Constants.Includes;
using Microsoft.IdentityModel.Tokens;

namespace CORE.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IOptions<Paths> _paths;
        private readonly IMapper _mapper;
        private readonly IGeoapifyService _geoapifyService;

        public VehicleService(IUnitOfWork unitOfWork, IFileService fileService, IOptions<Paths> paths, IMapper mapper, IGeoapifyService geoapifyService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _paths = paths;
            _mapper = mapper;
            _geoapifyService = geoapifyService;
        }
        private string? ValidateCreateVehicleDto(CreateVehicleDto dto)
        {
            if (dto.NumOfPassengers < 1)
                return "Number of passengers must be at least 1";
            if (dto.Year < 1900 || dto.Year > DateTime.UtcNow.Year)
                return "Year must be between 1900 and current year";
            if(dto.MainImage == null)
                return "Main image is required";
            if(dto.PricePerDay < 0 || dto.PricePerHour < 0 || dto.PricePerMonth < 0)
                return "Prices must be positive";
            if (dto.Location == null)
                return "Location is required";
            if(dto.VehicleBrandId == 0)
                return "Brand must be larger than 0 or null";
            if (dto.VehicleTypeId == 0)
                return "Type must be larger than 0 or null";
            return null;
        }
        public async Task<ResponseDto<GetVehicleDto>> CreateVehicleAsync(CreateVehicleDto createVehicleDto, int? renterId)
        {
            if(renterId == null || renterId.Value == 0)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "RenterId is required"
                };
            if(ValidateCreateVehicleDto(createVehicleDto) is string errorMsg)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = errorMsg
                };
            var vehicle = _mapper.Map<Vehicle>(createVehicleDto);
            if(vehicle == null)
            {
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Mapping failed"
                };
            }

            vehicle.MainImagePath = await _fileService.UploadFileAsync(_paths.Value.VehicleImages, null, createVehicleDto.MainImage, AllowedExtensions.ImageExtensions);
            if(vehicle.MainImagePath == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Failed to upload main image"
                };

            vehicle.RenterId = renterId.Value;
            vehicle.AddedAt = DateTime.UtcNow;
            if(createVehicleDto.Images != null)
            {
                var uploadTasks = createVehicleDto.Images.Select(img => Task.Run(() => _fileService.UploadFileAsync(_paths.Value.VehicleImages, null, img, AllowedExtensions.ImageExtensions)));
                var results = await Task.WhenAll(uploadTasks);
                if (results != null)
                    vehicle.VehicleImages = results.Select(r => new VehicleImage { ImagePath = r }).ToList();
            }
            await _unitOfWork.Vehicles.AddOrUpdateAsync(vehicle);
            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
            {
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to add vehicle"
                };
            }
            var addedVehicleResult = await GetVehicleByIdAsync(vehicle.Id);
            if(addedVehicleResult.StatusCode != StatusCodes.OK)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Vehicle added but failed to retrieve it"
                };
            return new ResponseDto<GetVehicleDto>
            {
                StatusCode = StatusCodes.Created,
                Message = "Vehicle added successfully",
                Data = addedVehicleResult.Data
            };
        }
        public async Task<ResponseDto<GetVehicleDto>> GetVehicleByIdAsync(int vehicleId)
        {
            string[] includes = { VehicleIncludes.VehicleBrand, VehicleIncludes.VehicleType, VehicleIncludes.Renter, VehicleIncludes.VehicleImages };
            var vehicle = await _unitOfWork.Vehicles.FindAsync(v=>v.Id == vehicleId, includes);
            
            if(vehicle == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "Vehicle not found"
                };

            return new ResponseDto<GetVehicleDto>
            {
                StatusCode = StatusCodes.OK,
                Data = _mapper.Map<GetVehicleDto>(vehicle)
            };
        }
        public async Task<ResponseDto<PagedResultDto<GetVehicleSummaryDto>>> GetNearestVehiclesAsync(int userId, int pageNo, int pageSize, string? searchKey, int? brandId, int? typeId, VehicleTransmission? transmission, double? Rate, double? minPrice, double? maxPrice, int? year, string baseUrl)
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
                    MainImagePath = $"{baseUrl}{v.MainImagePath}",
                    Distance = (6371 * Math.Acos(
                        Math.Cos(userLocation.Latitude * Math.PI / 180.0) *
                        Math.Cos(v.Location.Latitude * Math.PI / 180.0) *
                        Math.Cos((v.Location.Longitude - userLocation.Longitude) * Math.PI / 180.0) +
                        Math.Sin(userLocation.Latitude * Math.PI / 180.0) *
                        Math.Sin(v.Location.Latitude * Math.PI / 180.0)
                    )),
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
                TotalPages = PaginationHelpers.CalculateTotalPages(vehicles.Count(), pageSize)
            };

            return new ResponseDto<PagedResultDto<GetVehicleSummaryDto>>
            {
                Data = pagedData,
                Message = "Vehicles Loaded Successfully...",
                StatusCode = StatusCodes.OK,
            };
        }
        public async Task<ResponseDto<object>> DeleteVehicleByIdAsync(int vehicleId, int? renterId)
        {
            if(vehicleId == 0)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "VehicleId is required"
                };

            if (renterId == null || renterId == 0)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "RenterId is required"
                };

            string[] includes = { VehicleIncludes.VehicleImages };
            var vehicle = await _unitOfWork.Vehicles.FindAsync(v => v.Id == vehicleId, includes);

            if (vehicle == null)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "Vehicle not found"
                };
            else if(vehicle.RenterId != renterId)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "You are not the owner of this vehicle"
                };

            var vehicleImages = new List<string> { vehicle.MainImagePath };
            if(vehicle.VehicleImages != null && vehicle.VehicleImages.Count > 0)
                vehicleImages.AddRange(vehicle.VehicleImages.Select(vi => vi.ImagePath));
            if (vehicleImages.Count != 0)
                vehicleImages.ForEach(img => _fileService.DeleteFile(img));

            _unitOfWork.Vehicles.Delete(vehicle);
            var changes = await _unitOfWork.CommitAsync();

            if(changes == 0)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to delete vehicle"
                };

            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.OK,
                Message = "Vehicle deleted successfully"
            };
        }
    }
}
