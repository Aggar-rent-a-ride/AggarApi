using AutoMapper;
using CORE.DTOs.VehicleBrand;
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
