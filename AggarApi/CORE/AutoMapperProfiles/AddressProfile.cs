using AutoMapper;
using CORE.DTOs.Auth;
using CORE.DTOs.Geoapify;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class AddressProfile : Profile
    {
        public AddressProfile()
        {
            CreateMap<Address, GeoapifyAddressResponse>()
                .ForMember(dest => dest.State, act => act.MapFrom(src => src.Governorate))
                .ForMember(dest => dest.City, act => act.MapFrom(src => src.City))
                .ForMember(dest => dest.Street, act => act.MapFrom(src => src.Street))
                .ForMember(dest => dest.Country, act => act.MapFrom(src => src.Country))
                .ReverseMap();
        }
    }
}
