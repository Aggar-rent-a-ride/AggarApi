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
using CORE.Extensions;
using Microsoft.EntityFrameworkCore;
using CORE.Helpers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DATA.Constants.Includes;
using Microsoft.IdentityModel.Tokens;
using CORE.DTOs.Discount;

namespace CORE.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IOptions<Paths> _paths;
        private readonly IMapper _mapper;
        private readonly IGeoapifyService _geoapifyService;
        private readonly IReviewService _reviewService;

        public VehicleService(IUnitOfWork unitOfWork, IFileService fileService, IOptions<Paths> paths, IMapper mapper, IGeoapifyService geoapifyService, IReviewService reviewService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _paths = paths;
            _mapper = mapper;
            _geoapifyService = geoapifyService;
            _reviewService = reviewService;
        }
        private string? ValidateCreateVehicleDto(CreateVehicleDto dto)
        {
            if (dto.NumOfPassengers < 1)
                return "Number of passengers must be at least 1";
            if (dto.Year < 1900 || dto.Year > DateTime.UtcNow.Year)
                return "Year must be between 1900 and current year";
            if(dto.MainImage == null)
                return "Main image is required";
            if(dto.PricePerDay < 0)
                return "Prices must be positive";
            if (dto.Location == null)
                return "Location is required";
            if(dto.VehicleBrandId == 0)
                return "Brand must be larger than 0 or null";
            if (dto.VehicleTypeId == 0)
                return "Type must be larger than 0 or null";
            return null;
        }
        private string? ValidateUpdateVehicleDto(UpdateVehicleDto dto)
        {
            if(dto.Id == 0)
                return "VehicleId is required";
            if (dto.NumOfPassengers < 1)
                return "Number of passengers must be at least 1";
            if (dto.Year < 1900 || dto.Year > DateTime.UtcNow.Year)
                return "Year must be between 1900 and current year";
            if (dto.PricePerDay < 0)
                return "Prices must be positive";
            if (dto.Location == null)
                return "Location is required";
            if (dto.VehicleBrandId == 0)
                return "Brand must be larger than 0 or null";
            if (dto.VehicleTypeId == 0)
                return "Type must be larger than 0 or null";
            return null;
        }
        public async Task<ResponseDto<GetVehicleDto>> CreateVehicleAsync(CreateVehicleDto createVehicleDto, int? renterId)
        {
            if(renterId == null)
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
            string[] includes = { VehicleIncludes.VehicleBrand, VehicleIncludes.VehicleType, VehicleIncludes.Renter, VehicleIncludes.VehicleImages, VehicleIncludes.Discounts };
            var vehicle = await _unitOfWork.Vehicles.FindAsync(v=>v.Id == vehicleId, includes);
            
            if(vehicle == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "Vehicle not found"
                };


            var result = _mapper.Map<GetVehicleDto>(vehicle);
            result.Rate = (await _reviewService.GetVehicleTotalRateAsync(vehicle.Id)).Data;
            return new ResponseDto<GetVehicleDto>
            {
                StatusCode = StatusCodes.OK,
                Data = result
            };
        }
        public async Task<ResponseDto<PagedResultDto<GetVehicleSummaryDto>>> GetNearestVehiclesAsync(int userId, int pageNo, int pageSize, string? searchKey, int? brandId, int? typeId, VehicleTransmission? transmission, double? Rate, decimal? minPrice, decimal? maxPrice, int? year, Location? location = null)
        {
            if(userId == 0)
                return new ResponseDto<PagedResultDto<GetVehicleSummaryDto>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "UserId is required"
                };

            AppUser? user = await _unitOfWork.AppUsers.GetAsync(userId);
            if (user == null)
                return new ResponseDto<PagedResultDto<GetVehicleSummaryDto>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "User Not Found"
                };

            if(location == null)
                location = user.Location;

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
                    PricePerDay = v.PricePerDay,
                    Rate = v.Rate,
                    MainImagePath = v.MainImagePath,
                    Distance = (6371 * Math.Acos(
                        Math.Cos(location.Latitude * Math.PI / 180.0) *
                        Math.Cos(v.Location.Latitude * Math.PI / 180.0) *
                        Math.Cos((v.Location.Longitude - location.Longitude) * Math.PI / 180.0) +
                        Math.Sin(location.Latitude * Math.PI / 180.0) *
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

            if (renterId == null)
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
                    StatusCode = StatusCodes.Forbidden,
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
        public async Task<ResponseDto<GetVehicleDto>> UpdateVehicleAsync(UpdateVehicleDto updateVehicleDto, int? renterId)
        {
            if (renterId == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "RenterId is required"
                };
            

            if(ValidateUpdateVehicleDto(updateVehicleDto) is string errorMsg)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = errorMsg
                };

            var vehicle = await _unitOfWork.Vehicles.GetAsync(updateVehicleDto.Id);
            if (vehicle == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "Vehicle not found"
                };
            
            if(renterId != vehicle.RenterId)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.Forbidden,
                    Message = "You are not the owner of this vehicle"
                };

            _mapper.Map(updateVehicleDto, vehicle);
            if (updateVehicleDto.MainImage != null)
            {
                vehicle.MainImagePath = await _fileService.UploadFileAsync(_paths.Value?.VehicleImages, vehicle.MainImagePath, updateVehicleDto.MainImage, AllowedExtensions.ImageExtensions);
                if (vehicle.MainImagePath == null)
                    return new ResponseDto<GetVehicleDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Failed to upload main image"
                    };
            }
            
            await _unitOfWork.Vehicles.AddOrUpdateAsync(vehicle);
            var changes = await _unitOfWork.CommitAsync();
            if(changes == 0)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to update vehicle"
                };
            
            var updatedVehicleResult = await GetVehicleByIdAsync(vehicle.Id);
            if (updatedVehicleResult.StatusCode != StatusCodes.OK)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Vehicle updated but failed to retrieve it"
                };
            
            return new ResponseDto<GetVehicleDto>
            {
                StatusCode = StatusCodes.OK,
                Message = "Vehicle updated successfully",
                Data = updatedVehicleResult.Data
            };
        }
        public async Task<ResponseDto<GetVehicleDto>> UpdateVehicleImagesAsync(UpdateVehicleImagesDto updateVehicleImagesDto, int? renterId)
        {
            if(renterId == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "RenterId is required"
                };

            if (updateVehicleImagesDto.VehicleId == 0)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "VehicleId is required"
                };

            string[] includes = { VehicleIncludes.VehicleImages };
            var vehicle = await _unitOfWork.Vehicles.FindAsync(v => v.Id == updateVehicleImagesDto.VehicleId, includes);
            if (vehicle == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "Vehicle not found"
                };

            if (renterId != vehicle.RenterId)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.Forbidden,
                    Message = "You are not the owner of this vehicle"
                };
            if(updateVehicleImagesDto.RemovedImagesPaths != null && updateVehicleImagesDto.RemovedImagesPaths.Count > 0)
            {
                updateVehicleImagesDto?.RemovedImagesPaths.ForEach(img => _fileService.DeleteFile(img));
                if(vehicle.VehicleImages != null)
                    vehicle.VehicleImages = vehicle.VehicleImages.Where(vi => updateVehicleImagesDto.RemovedImagesPaths.Contains(vi.ImagePath) == false).ToList();
            }
            if (updateVehicleImagesDto.NewImages != null && updateVehicleImagesDto.NewImages.Count > 0)
            {
                var uploadTasks = updateVehicleImagesDto.NewImages.Select(img => Task.Run(() => _fileService.UploadFileAsync(_paths.Value.VehicleImages, null, img, AllowedExtensions.ImageExtensions)));
                var results = await Task.WhenAll(uploadTasks);
                if (results != null)
                {
                    if (vehicle.VehicleImages == null)
                        vehicle.VehicleImages = new List<VehicleImage>();
                    results.ToList().ForEach(r => vehicle.VehicleImages.Add(new VehicleImage { ImagePath = r }));
                }
            }
            await _unitOfWork.Vehicles.AddOrUpdateAsync(vehicle);
            var changes = await _unitOfWork.CommitAsync();
            if (changes == 0)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to update vehicle images"
                };
            var updatedVehicleResult = await GetVehicleByIdAsync(vehicle.Id);
            if (updatedVehicleResult.StatusCode != StatusCodes.OK)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Vehicle images updated but failed to retrieve it"
                };
            return new ResponseDto<GetVehicleDto>
            {
                StatusCode = StatusCodes.OK,
                Message = "Vehicle images updated successfully",
                Data = updatedVehicleResult.Data
            };
        }
        public async Task<ResponseDto<GetVehicleDto>> UpdateVehicleDiscountsAsync(UpdateVehicleDiscountsDto updateVehicleDiscountsDto, int? renterId)
        {
            if (renterId == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "RenterId is required"
                };

            var includes = new string[] { VehicleIncludes.Discounts };
            var vehicle = await _unitOfWork.Vehicles.FindAsync(v=>v.Id==updateVehicleDiscountsDto.VehicleId, includes);
            
            if (vehicle == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "Vehicle not found"
                };

            if (renterId != vehicle.RenterId)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.Forbidden,
                    Message = "You are not the owner of this vehicle"
                };

            vehicle.Discounts = _mapper.Map<List<Discount>>(updateVehicleDiscountsDto.Discounts);

            await _unitOfWork.Vehicles.AddOrUpdateAsync(vehicle);
            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to update vehicle discounts"
                };

            var updatedVehicleResult = await GetVehicleByIdAsync(vehicle.Id);
            
            if (updatedVehicleResult.StatusCode != StatusCodes.OK)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Vehicle discounts updated but failed to retrieve it"
                };

            return new ResponseDto<GetVehicleDto>
            {
                StatusCode = StatusCodes.OK,
                Message = "Vehicle discounts updated successfully",
                Data = updatedVehicleResult.Data
            };
        }
        public async Task<ResponseDto<GetVehicleDto>> GetVehicleByRentalIdAsync(int rentalId)
        {
            var vehicle = await _unitOfWork.Vehicles.GetVehicleByRentalIdAsync(rentalId);
            if (vehicle == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "Vehicle not found"
                };

            var result = _mapper.Map<GetVehicleDto>(vehicle);
            result.Rate = (await _reviewService.GetVehicleTotalRateAsync(vehicle.Id)).Data;
            return new ResponseDto<GetVehicleDto>
            {
                StatusCode = StatusCodes.OK,
                Data = result
            };
        }
    }
}
