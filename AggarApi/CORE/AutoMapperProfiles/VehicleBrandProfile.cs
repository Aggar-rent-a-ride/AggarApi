using AutoMapper;
using CORE.DTOs.VehicleBrand;
using CORE.DTOs.VehicleType;
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
            CreateMap<CreateVehicleBrandDto, VehicleBrand>()
                .ForMember(dest => dest.LogoPath, opt => opt.Ignore());
            CreateMap<UpdateVehicleBrandDto, VehicleBrand>()
                .ForMember(dest => dest.LogoPath, opt => opt.Ignore());
        }
    }
}
