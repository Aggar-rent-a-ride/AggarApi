using AutoMapper;
using CORE.DTOs.Vehicle;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class VehicleBrandProfile : Profile
    {
        public VehicleBrandProfile()
        {
            CreateMap<VehicleBrand, VehicleBrandDto>()
                .ReverseMap();
        }
    }
}
