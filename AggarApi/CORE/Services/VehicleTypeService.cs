using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.VehicleBrand;
using CORE.DTOs.VehicleType;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using Microsoft.Extensions.Logging;
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

        public VehicleTypeService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<VehicleTypeService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
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
