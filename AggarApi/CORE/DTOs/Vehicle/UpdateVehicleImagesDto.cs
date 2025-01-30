using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Vehicle
{
    public class UpdateVehicleImagesDto
    {
        public int VehicleId { get; set; }
        public List<string>? RemovedImagesPaths { get; set; }
        public List<IFormFile>? AddedImages { get; set; }
    }
}
