using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.VehicleBrand;
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
    public class VehicleBrandService : IVehicleBrandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<VehicleBrandService> _logger;

        public VehicleBrandService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<VehicleBrandService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseDto<List<VehicleBrandDto>>> GetAllAsync()
        {
            var brands = await _unitOfWork.VehicleBrands.GetAllAsync(b => b.Id > 0);
            if(brands == null)
            {
                _logger.LogError("No brands found");
                return new ResponseDto<List<VehicleBrandDto>>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "No brands found"
                };
            }
            _logger.LogInformation("All brands fetched successfully");

            var dto = _mapper.Map<List<VehicleBrandDto>>(brands);

            return new ResponseDto<List<VehicleBrandDto>>
            {
                StatusCode = StatusCodes.OK,
                Data = dto
            };

        }
    }
}
