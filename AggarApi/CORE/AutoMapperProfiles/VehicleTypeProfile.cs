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
            CreateMap<CreateVehicleTypeDto, VehicleType>()
                .ForMember(dest => dest.SlogenPath, opt => opt.Ignore());            
            CreateMap<UpdateVehicleTypeDto, VehicleType>()
                .ForMember(dest => dest.SlogenPath, opt => opt.Ignore());
        }
    }
}
