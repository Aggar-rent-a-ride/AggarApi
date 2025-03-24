using CORE.DTOs;
using CORE.DTOs.VehicleBrand;
using CORE.DTOs.VehicleType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IVehicleTypeService
    {
        Task<ResponseDto<List<VehicleTypeDto>>> GetAllAsync();
    }
}
