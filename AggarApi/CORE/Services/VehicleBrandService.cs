using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Paths;
using CORE.DTOs.VehicleBrand;
using CORE.DTOs.VehicleType;
using CORE.Services.IServices;
using DATA.Constants;
using DATA.DataAccess.Repositories.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class VehicleBrandService : IVehicleBrandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<VehicleBrandService> _logger;
        private readonly IFileService _fileService;
        private readonly IOptions<Paths> _paths;
        public VehicleBrandService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<VehicleBrandService> logger, IFileService fileService, IOptions<Paths> paths)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
            _paths = paths;
        }

        public async Task<ResponseDto<VehicleBrandDto>> CreateAsync(CreateVehicleBrandDto dto)
        {
            var vehicleBrand = _mapper.Map<DATA.Models.VehicleBrand>(dto);
            if (dto.Logo != null)
                vehicleBrand.LogoPath = await _fileService.UploadFileAsync(_paths.Value.VehicleBrands, null, dto.Logo, AllowedExtensions.ImageExtensions);

            await _unitOfWork.VehicleBrands.AddOrUpdateAsync(vehicleBrand);
            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
            {
                _logger.LogError("Failed to create vehicle brand: {Name}", dto.Name);
                return new ResponseDto<VehicleBrandDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to create vehicle brand"
                };
            }

            _logger.LogInformation("Creating new vehicle brand: {Name}", dto.Name);
            return new ResponseDto<VehicleBrandDto>
            {
                StatusCode = StatusCodes.Created,
                Data = _mapper.Map<VehicleBrandDto>(vehicleBrand),
                Message = "Vehicle brand created successfully"
            };
        }

        public async Task<ResponseDto<object>> DeleteAsync(int id)
        {
            var vehicleBrand = await _unitOfWork.VehicleBrands.GetAsync(id);
            if (vehicleBrand == null)
            {
                _logger.LogWarning("Vehicle brand with that id: {id} wasn't found", id);
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Vehicle brand with that id wasn't found"
                };
            }

            _unitOfWork.VehicleBrands.Delete(vehicleBrand);
            var changes = await _unitOfWork.CommitAsync();
            if (changes == 0)
            {
                _logger.LogError("Couldn't Delete Vehicle Brand with Id: {id}", id);
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Couldn't Delete Vehicle Brand with that Id"
                };
            }
            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.OK,
            };
        }

        public async Task<ResponseDto<List<VehicleBrandDto>>> GetAllAsync()
        {
            var brands = await _unitOfWork.VehicleBrands.GetAllAsync(b => b.Id > 0);

            _logger.LogInformation("All brands fetched successfully");

            var dto = _mapper.Map<List<VehicleBrandDto>>(brands);

            return new ResponseDto<List<VehicleBrandDto>>
            {
                StatusCode = StatusCodes.OK,
                Data = dto
            };

        }

        public async Task<ResponseDto<VehicleBrandDto>> UpdateAsync(UpdateVehicleBrandDto dto)
        {
            var vehicleBrand = await _unitOfWork.VehicleBrands.GetAsync(dto.Id);
            if (vehicleBrand == null)
            {
                _logger.LogWarning("Failed to get vehicle brand with that id: {id}", dto.Id);
                return new ResponseDto<VehicleBrandDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to get vehicle brand with that id"
                };
            }
            vehicleBrand.Name = dto.Name;
            vehicleBrand.Country = dto.Country;
            if (dto.Logo != null)
                vehicleBrand.LogoPath = await _fileService.UploadFileAsync(_paths.Value.VehicleBrands, null, dto.Logo, AllowedExtensions.ImageExtensions);

            await _unitOfWork.VehicleBrands.AddOrUpdateAsync(vehicleBrand);
            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
            {
                _logger.LogError("Failed to update vehicle brand: {Name}", dto.Name);
                return new ResponseDto<VehicleBrandDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to update vehicle brand"
                };
            }

            _logger.LogInformation("updating vehicle brand: {Name}", dto.Name);
            return new ResponseDto<VehicleBrandDto>
            {
                StatusCode = StatusCodes.OK,
                Data = _mapper.Map<VehicleBrandDto>(vehicleBrand),
                Message = "Vehicle brand updated successfully"
            };
        }
    }
}
