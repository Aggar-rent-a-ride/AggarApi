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
    public interface IVehicleBrandService
    {
        Task<ResponseDto<List<VehicleBrandDto>>> GetAllAsync();
        Task<ResponseDto<VehicleBrandDto>> CreateAsync(CreateVehicleBrandDto dto);
        Task<ResponseDto<VehicleBrandDto>> UpdateAsync(UpdateVehicleBrandDto dto);
        Task<ResponseDto<object>> DeleteAsync(int id);
    }
}
