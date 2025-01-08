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
    public class RenterProfile : Profile
    {
        public RenterProfile()
        {
            CreateMap<Renter, RegisterDto>()
                .IncludeBase<AppUser, RegisterDto>() 
                .ReverseMap()
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore());
        }
    }
}
