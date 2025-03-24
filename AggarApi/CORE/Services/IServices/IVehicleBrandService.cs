using CORE.DTOs;
using CORE.DTOs.VehicleBrand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IVehicleBrandService
    {
        Task<ResponseDto<List<VehicleBrandDto>>> GetAllAsync();
    }
}
