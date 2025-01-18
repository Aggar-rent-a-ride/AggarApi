using CORE.DTOs;
using CORE.DTOs.Vehicle;
using CORE.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class VehicleService : IVehicleService
    {
        public Task<ResponseDto<GetVehicleDto>> CreateVehicleAsync(CreateVehicleDto createVehicleDto)
        {
            throw new NotImplementedException();
        }
    }
}
