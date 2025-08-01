﻿using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using DATA.Models.Enums;
using CORE.DTOs;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

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
        private readonly IMemoryCache _memoryCache;

        public VehicleService(IUnitOfWork unitOfWork, IFileService fileService, IOptions<Paths> paths, IMapper mapper, IGeoapifyService geoapifyService, IReviewService reviewService, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _paths = paths;
            _mapper = mapper;
            _geoapifyService = geoapifyService;
            _reviewService = reviewService;
            _memoryCache = memoryCache;
        }
        private string? ValidateCreateVehicleDto(CreateVehicleDto dto)
        {
            if (dto.NumOfPassengers < 1)
                return "Number of passengers must be at least 1";
            if (dto.Year < 1900 || dto.Year > DateTime.UtcNow.Year)
                return "Year must be between 1900 and current year";
            if (dto.MainImage == null)
                return "Main image is required";
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
        private string? ValidateUpdateVehicleDto(UpdateVehicleDto dto)
        {
            if (dto.Id == 0)
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
            if (renterId == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "RenterId is required"
                };
            if (ValidateCreateVehicleDto(createVehicleDto) is string errorMsg)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = errorMsg
                };
            var vehicle = _mapper.Map<Vehicle>(createVehicleDto);
            if (vehicle == null)
            {
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Mapping failed"
                };
            }

            vehicle.MainImagePath = await _fileService.UploadFileAsync(_paths.Value.VehicleImages, null, createVehicleDto.MainImage, AllowedExtensions.ImageExtensions);
            if (vehicle.MainImagePath == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Failed to upload main image"
                };

            vehicle.RenterId = renterId.Value;
            vehicle.AddedAt = DateTime.UtcNow;
            if (createVehicleDto.Images != null)
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
            var addedVehicleResult = await GetVehicleByIdAsync(vehicle.Id, null);
            if (addedVehicleResult.StatusCode != StatusCodes.OK)
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
        public async Task<ResponseDto<GetVehicleDto>> GetVehicleByIdAsync(int vehicleId, int? userId)
        {
            string[] includes = { VehicleIncludes.VehicleBrand, VehicleIncludes.VehicleType, VehicleIncludes.Renter, VehicleIncludes.VehicleImages, VehicleIncludes.Discounts };
            var vehicle = await _unitOfWork.Vehicles.FindAsync(v => v.Id == vehicleId, includes);

            if (vehicle == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Vehicle not found"
                };


            var result = _mapper.Map<GetVehicleDto>(vehicle);
            if(userId != null)
                result.IsFavourite = await _unitOfWork.CustomersFavoriteVehicles.CheckAnyAsync(c => c.CustomerId == userId && c.VehicleId == vehicleId);
            
            return new ResponseDto<GetVehicleDto>
            {
                StatusCode = StatusCodes.OK,
                Data = result
            };
        }
        public async Task<ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>> GetVehiclesAsync(int userId, VehiclesSearchQuery searchQuery)
        {
            if (userId == 0)
                return new ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "UserId is required"
                };

            AppUser? user = await _unitOfWork.AppUsers.GetAsync(userId);
            if (user == null)
                return new ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "User Not Found"
                };

            if (searchQuery.minPrice > searchQuery.maxPrice)
                return new ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Min Price can't be greater that Max Price"
                };

            Location location = null;
            if (searchQuery.latitude == null || searchQuery.longitude == null)
                location = user.Location;
            else
            {
                location = new Location
                {
                    Latitude = searchQuery.latitude.Value,
                    Longitude = searchQuery.longitude.Value
                };
            }

            searchQuery.pageNo = searchQuery.pageNo <= 0 ? 1 : searchQuery.pageNo;
            searchQuery.pageSize = searchQuery.pageSize <= 0 ? 1 : searchQuery.pageSize;

            IQueryable<Vehicle> vehicles = _unitOfWork.Vehicles.GetVehicles(searchQuery.brandId, searchQuery.typeId, searchQuery.transmission, searchQuery.searchKey, searchQuery.minPrice, searchQuery.maxPrice, searchQuery.year, searchQuery.rate);

            vehicles = vehicles.Where(v => v.Status == VehicleStatus.Active);

            var vehiclesSummary = vehicles
                .Select(v => new GetVehicleSummaryDto
                {
                    Id = v.Id,
                    Brand = v.VehicleBrand != null ? v.VehicleBrand.Name : null,
                    Type = v.VehicleType != null ? v.VehicleType.Name : null,
                    Model = v.Model,
                    Year = v.Year,
                    PricePerDay = v.PricePerDay,
                    Rate = v.Rate,
                    Transmission = v.Transmission,
                    MainImagePath = v.MainImagePath,
                    Distance = (6371 * Math.Acos(
                        Math.Cos(location.Latitude * Math.PI / 180.0) *
                        Math.Cos(v.Location.Latitude * Math.PI / 180.0) *
                        Math.Cos((v.Location.Longitude - location.Longitude) * Math.PI / 180.0) +
                        Math.Sin(location.Latitude * Math.PI / 180.0) *
                        Math.Sin(v.Location.Latitude * Math.PI / 180.0)
                    )),
                });

            if (searchQuery.byNearest == true)
            {
                vehiclesSummary = vehiclesSummary.OrderBy(dto => dto.Distance);
            }

            var data = await vehiclesSummary
                .Skip((searchQuery.pageNo - 1) * searchQuery.pageSize)
                .Take(searchQuery.pageSize)
                .ToListAsync();

            var pagedData = new PagedResultDto<IEnumerable<GetVehicleSummaryDto>>
            {
                Data = data,
                PageNumber = searchQuery.pageNo,
                PageSize = searchQuery.pageSize,
                TotalPages = PaginationHelpers.CalculateTotalPages(vehicles.Count(), searchQuery.pageSize)
            };

            return new ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>
            {
                Data = pagedData,
                Message = "Vehicles Loaded Successfully...",
                StatusCode = StatusCodes.OK,
            };
        }
        public async Task<ResponseDto<object>> DeleteVehicleByIdAsync(int vehicleId, int? renterId, string[] roles)
        {
            if (vehicleId == 0)
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
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Vehicle not found"
                };

            if (roles.Contains(Roles.Admin) == false && vehicle.RenterId != renterId)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.Forbidden,
                    Message = "You are not the owner of this vehicle"
                };
            }

            var vehicleImages = new List<string> { vehicle.MainImagePath };
            if (vehicle.VehicleImages != null && vehicle.VehicleImages.Count > 0)
                vehicleImages.AddRange(vehicle.VehicleImages.Select(vi => vi.ImagePath));
            if (vehicleImages.Count != 0)
                vehicleImages.ForEach(img => _fileService.DeleteFile(img));

            _unitOfWork.Vehicles.Delete(vehicle);
            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
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


            if (ValidateUpdateVehicleDto(updateVehicleDto) is string errorMsg)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = errorMsg
                };

            var vehicle = await _unitOfWork.Vehicles.GetAsync(updateVehicleDto.Id);
            if (vehicle == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Vehicle not found"
                };

            if (renterId != vehicle.RenterId)
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
            if (changes == 0)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to update vehicle"
                };

            var updatedVehicleResult = await GetVehicleByIdAsync(vehicle.Id, null);
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
            if (renterId == null)
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
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Vehicle not found"
                };

            if (renterId != vehicle.RenterId)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.Forbidden,
                    Message = "You are not the owner of this vehicle"
                };
            if (updateVehicleImagesDto.RemovedImagesPaths != null && updateVehicleImagesDto.RemovedImagesPaths.Count > 0)
            {
                updateVehicleImagesDto?.RemovedImagesPaths.ForEach(img => _fileService.DeleteFile(img));
                if (vehicle.VehicleImages != null)
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
            var updatedVehicleResult = await GetVehicleByIdAsync(vehicle.Id, null);
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
            var vehicle = await _unitOfWork.Vehicles.FindAsync(v => v.Id == updateVehicleDiscountsDto.VehicleId, includes);

            if (vehicle == null)
                return new ResponseDto<GetVehicleDto>
                {
                    StatusCode = StatusCodes.BadRequest,
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

            var updatedVehicleResult = await GetVehicleByIdAsync(vehicle.Id, null);

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
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Vehicle not found"
                };

            var result = _mapper.Map<GetVehicleDto>(vehicle);

            return new ResponseDto<GetVehicleDto>
            {
                StatusCode = StatusCodes.OK,
                Data = result
            };
        }
        public async Task<ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>> GetVehiclesByStatusAsync(VehicleStatus status, int pageNo, int pageSize, int maxPageSize = 100)
        {
            if (PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string errorMsg)
                return new ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = errorMsg
                };

            var includes = new string[] { VehicleIncludes.VehicleBrand, VehicleIncludes.VehicleType };
            var vehicles = await _unitOfWork.Vehicles.FindAsync(v => v.Status == status, pageNo, pageSize, includes);
            var count = await _unitOfWork.Vehicles.CountAsync(v => v.Status == status);


            return new ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>
            {
                StatusCode = StatusCodes.OK,
                Data = PaginationHelpers.CreatePagedResult(_mapper.Map<IEnumerable<GetVehicleSummaryDto>>(vehicles), pageNo, pageSize, count)
            };
        }
        public async Task<ResponseDto<int>> GetVehiclesByStatusCountAsync(VehicleStatus status)
        {
            var count = 0;
            if (status == VehicleStatus.Active)
                count = await _unitOfWork.Vehicles.CountAsync(v => v.Status == VehicleStatus.Active);
            else
                count = await _unitOfWork.Vehicles.CountAsync(v => v.Status == VehicleStatus.OutOfService);

            return new ResponseDto<int>
            {
                StatusCode = StatusCodes.OK,
                Data = count
            };
        }
        private string GenerateGetMostRentedVehiclesAsyncCacheKey(int pageNo, int pageSize) =>
            $"GetMostRentedVehiclesAsync_{pageNo}_{pageSize}";
        private string GenerateGetPopularVehiclesAsyncCacheKey(int pageNo, int pageSize) =>
            $"GetPopularVehiclesAsync_{pageNo}_{pageSize}";
        public async Task<ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>> GetMostRentedVehiclesAsync(int pageNo, int pageSize, int maxPageSize = 50)
        {

            if (PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string errorMsg)
                return new ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = errorMsg
                };

            var includes = new string[] { VehicleIncludes.VehicleBrand, VehicleIncludes.VehicleType };
            IEnumerable<Vehicle> vehicles = await _unitOfWork.Vehicles.GetMostRentedVehiclesAsync(pageNo, pageSize);
            var count = await _unitOfWork.Vehicles.GetMostRentedVehiclesCountAsync();

            return new ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>
            {
                StatusCode = StatusCodes.OK,
                Data = PaginationHelpers.CreatePagedResult(_mapper.Map<IEnumerable<GetVehicleSummaryDto>>(vehicles), pageNo, pageSize, count)
            };
        }
        public async Task<ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>> GetPopularVehiclesAsync(int pageNo, int pageSize, int maxPageSize = 50)
        {
            if (PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string errorMsg)
                return new ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = errorMsg
                };

            var includes = new string[] { VehicleIncludes.VehicleBrand, VehicleIncludes.VehicleType };
            IEnumerable<Vehicle> vehicles = await _unitOfWork.Vehicles.GetPopularVehiclesAsync(pageNo, pageSize);
            var count = await _unitOfWork.Vehicles.GetPopularVehiclesCountAsync();

            return new ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>
            {
                StatusCode = StatusCodes.OK,
                Data = PaginationHelpers.CreatePagedResult(_mapper.Map<IEnumerable<GetVehicleSummaryDto>>(vehicles), pageNo, pageSize, count)
            };
        }
        private async Task<ResponseDto<object>> SetVehicleAsFavourite(Vehicle vehicle, Customer customer, bool isFavourite)
        {
            if (isFavourite)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.Conflict,
                    Message = "Vehicle is already marked as favourite for this user"
                };
            }

            vehicle.FavoriteCustomers.Add(customer);

            int changes = await _unitOfWork.CommitAsync();

            if (changes > 0)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.OK,
                    Message = "Vehicle added to favourites"
                };
            }

            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.InternalServerError,
                Message = "Failed to add vehicle to favourites"
            };
        }
        private  async Task<ResponseDto<object>> UnSetVehicleAsFavourite(Vehicle vehicle, Customer customer, bool isFavourite)
        {
            if (!isFavourite)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.Conflict,
                    Message = "Vehicle is already not marked as favourite"
                };
            }

            vehicle.FavoriteCustomers.Remove(customer);

            int changes = await _unitOfWork.CommitAsync();

            if (changes > 0)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.OK,
                    Message = "Vehicle removed from favourites"
                };
            }

            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.InternalServerError,
                Message = "Failed to remove vehicle from favourites"
            };
        }
        public async Task<ResponseDto<object>> VehicleFavouriteAsync(int customerId, SetVehicleFavouriteDto dto)
        {
            Customer? customer = await _unitOfWork.Customers.GetAsync(customerId);

            if (customer == null)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.Unauthorized,
                    Message = "User Not Found"
                };
            }

            Vehicle? vehicle = await _unitOfWork.Vehicles.GetAsync(dto.VehicleId, [VehicleIncludes.FavoriteCustomers]);

            if (vehicle == null)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Vehicle Not Found"
                };
            }

            bool isFavourite = vehicle.FavoriteCustomers != null && vehicle.FavoriteCustomers.Any(f => f.Id == customerId);

            if (dto.IsFavourite)
            {
                return await SetVehicleAsFavourite(vehicle, customer, isFavourite);
            }
            else
            {
                return await UnSetVehicleAsFavourite(vehicle, customer, isFavourite);
            }

        }

        public async Task<ResponseDto<PagedResultDto<IEnumerable<RenterVehiclesDto>>>> GetRenterVehiclesAsync(int renterId, int pageNo, int pageSize)
        {
            pageNo = pageNo > 0 ? pageNo : 1;
            pageSize = pageSize > 0 ? pageSize : 1;

            Renter? renter = await _unitOfWork.Renters.GetAsync(renterId);
            if(renter == null)
            {
                return new ResponseDto<PagedResultDto<IEnumerable<RenterVehiclesDto>>>
                {
                    Message = "Renter Id is required",
                    StatusCode = StatusCodes.Unauthorized,
                };
            }

            var vehicles = await _unitOfWork.Vehicles.FindAsync(v => v.RenterId == renterId, pageNo, pageSize, [VehicleIncludes.VehicleBrand, VehicleIncludes.VehicleType]);
            int count = await _unitOfWork.Vehicles.CountAsync(v => v.RenterId == renterId);

            return new ResponseDto<PagedResultDto<IEnumerable<RenterVehiclesDto>>>
            {
                StatusCode = StatusCodes.OK,
                Data = PaginationHelpers.CreatePagedResult(_mapper.Map<IEnumerable<RenterVehiclesDto>>(vehicles), pageNo, pageSize, count)
            };
        }

        public async Task<ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>> GetCustomerFavouriteVehiclesAsync(int customerId, int pageNo, int pageSize)
        {
            pageNo = pageNo > 0 ? pageNo : 1;
            pageSize = pageSize > 0 ? pageSize : 1;

            Customer? customer = await _unitOfWork.Customers.GetAsync(customerId);
            if (customer == null)
            {
                return new ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>
                {
                    Message = "Customer Id is required",
                    StatusCode = StatusCodes.Unauthorized,
                };
            }

            IQueryable<Vehicle> vehicles = _unitOfWork.Vehicles.GetFavouriteVehicles(customerId);

            var vehiclesSummary = vehicles
                .Select(v => new GetVehicleSummaryDto
                {
                    Id = v.Id,
                    Brand = v.VehicleBrand != null ? v.VehicleBrand.Name : null,
                    Type = v.VehicleType != null ? v.VehicleType.Name : null,
                    Model = v.Model,
                    Year = v.Year,
                    PricePerDay = v.PricePerDay,
                    Rate = v.Rate,
                    Transmission = v.Transmission,
                    MainImagePath = v.MainImagePath,
                    Distance = (6371 * Math.Acos(
                        Math.Cos(customer.Location.Latitude * Math.PI / 180.0) *
                        Math.Cos(v.Location.Latitude * Math.PI / 180.0) *
                        Math.Cos((v.Location.Longitude - customer.Location.Longitude) * Math.PI / 180.0) +
                        Math.Sin(customer.Location.Latitude * Math.PI / 180.0) *
                        Math.Sin(v.Location.Latitude * Math.PI / 180.0)
                    )),
                });

            var data = await vehiclesSummary
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedData = new PagedResultDto<IEnumerable<GetVehicleSummaryDto>>
            {
                Data = data,
                PageNumber = pageNo,
                PageSize = pageSize,
                TotalPages = PaginationHelpers.CalculateTotalPages(vehicles.Count(), pageSize)
            };

            return new ResponseDto<PagedResultDto<IEnumerable<GetVehicleSummaryDto>>>
            {
                Data = pagedData,
                Message = "Vehicles Loaded Successfully...",
                StatusCode = StatusCodes.OK,
            };
        }

    }
}
