using AutoMapper;
using CORE.DTOs.VehicleType;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class VehicleTypeProfile : Profile
    {
        public VehicleTypeProfile()
        {
            CreateMap<VehicleType, VehicleTypeDto>()
                .ReverseMap();
        }
    }
}
