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
using DATA.Constants.Includes;

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
        public async Task<ResponseDto<GetVehicleDto>> CreateVehicleAsync(CreateVehicleDto createVehicleDto, int? renterId)
        {
            if(renterId == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "RenterId is required"
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
            vehicle.Address = _mapper.Map<Address>(await _geoapifyService.GetAddressByLocationAsync(createVehicleDto.Location));
            vehicle.RenterId = renterId.Value;
            vehicle.AddedAt = DateTime.UtcNow;
            var uploadTasks = createVehicleDto.Images.Select(img => Task.Run(() => _fileService.UploadFileAsync(_paths.Value.VehicleImages, null, img, AllowedExtensions.ImageExtensions)));
            var results = await Task.WhenAll(uploadTasks);
            if(results != null)
                vehicle.VehicleImages = results.Select(r => new VehicleImage { ImagePath = r}).ToList();

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
                    PricePerDay = v.PricePerDay,
                    Rate = v.Rate,
                    MainImagePath = v.MainImagePath,
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
    }
}
