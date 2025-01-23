using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using DATA.Models.Enums;
﻿using CORE.DTOs;
using CORE.DTOs.Vehicle;
using CORE.Services.IServices;
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

        public Task<Vehicle> GetNearestVehicles(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<Vehicle> GetNearestVehiclesByCriteria(int userId, string? searchKey, int? typeId, int? brandId, PriceCategory? priceCategory, double? minPrice, double? maxPrice, string? model, int? year, double? Rate)
        {
            throw new NotImplementedException();
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
            string[] includes = {Includes.VehicleBrands, Includes.VehicleTypes};
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
    }
}
