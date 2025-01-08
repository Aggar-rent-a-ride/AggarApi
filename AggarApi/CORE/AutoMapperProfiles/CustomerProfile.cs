using AutoMapper;
using CORE.DTOs.Auth;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, RegisterDto>()
                .IncludeBase<AppUser, RegisterDto>() 
                .ReverseMap()
                .ForMember(dest => dest.RecommendedBrands, opt => opt.Ignore())
                .ForMember(dest => dest.RecommendedTypes, opt => opt.Ignore())
                .ForMember(dest => dest.FavoriteVehicles, opt => opt.Ignore())
                .ForMember(dest => dest.Bookings, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore());
        }
    }
}
