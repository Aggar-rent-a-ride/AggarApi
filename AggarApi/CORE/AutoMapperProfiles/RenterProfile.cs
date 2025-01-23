using AutoMapper;
using CORE.DTOs.Auth;
using CORE.DTOs.Vehicle;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class RenterProfile : Profile
    {
        public RenterProfile()
        {
            CreateMap<Renter, RegisterDto>()
                .IncludeBase<AppUser, RegisterDto>() 
                .ReverseMap()
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore());

            CreateMap<Renter, GetVehicleDtoRenterDto>()
                .ForMember(dest => dest.Id, act => act.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, act => act.MapFrom(src => src.Name))
                .ForMember(dest => dest.ImagePath, act => act.MapFrom(src => src.ImagePath))
                .ForMember(dest => dest.Rate, act => act.MapFrom(src => src.Rate))
                .ReverseMap();
        }
    }
}
