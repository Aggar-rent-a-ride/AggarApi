using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Paths;
using CORE.DTOs.Vehicle;
using CORE.DTOs.VehicleBrand;
using CORE.DTOs.VehicleType;
using CORE.Services.IServices;
using DATA.Constants;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class VehicleTypeService : IVehicleTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<VehicleTypeService> _logger;
        private readonly IFileService _fileService;
        private readonly IOptions<Paths> _paths;

        public VehicleTypeService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<VehicleTypeService> logger, IFileService fileService, IOptions<Paths> paths)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
            _paths = paths;
        }

        public async Task<ResponseDto<VehicleTypeDto>> CreateAsync(CreateVehicleTypeDto dto)
        {
            var vehicleType = _mapper.Map<DATA.Models.VehicleType>(dto);
            if(dto.Slogan != null)
                vehicleType.SlogenPath = await _fileService.UploadFileAsync(_paths.Value.VehicleTypes, null, dto.Slogan, AllowedExtensions.ImageExtensions);

            await _unitOfWork.VehicleTypes.AddOrUpdateAsync(vehicleType);
            var changes = await _unitOfWork.CommitAsync();

            if(changes == 0)
            {
                _logger.LogError("Failed to create vehicle type: {Name}", dto.Name);
                return new ResponseDto<VehicleTypeDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to create vehicle type"
                };
            }

            _logger.LogInformation("Creating new vehicle type: {Name}", dto.Name);
            return new ResponseDto<VehicleTypeDto>
            {
                StatusCode = StatusCodes.Created,
                Data = _mapper.Map<VehicleTypeDto>(vehicleType),
                Message = "Vehicle type created successfully"
            };
        }

        public async Task<ResponseDto<List<VehicleTypeDto>>> GetAllAsync()
        {
            var types = await _unitOfWork.VehicleTypes.GetAllAsync(t => t.Id > 0);

            _logger.LogInformation("All types fetched successfully");

            var dto = _mapper.Map<List<VehicleTypeDto>>(types);

            return new ResponseDto<List<VehicleTypeDto>>
            {
                StatusCode = StatusCodes.OK,
                Data = dto
            };

        }
    }
}
