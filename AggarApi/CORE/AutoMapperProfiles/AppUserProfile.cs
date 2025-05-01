using AutoMapper;
using CORE.DTOs.AppUser;
using CORE.DTOs.Auth;
using CORE.DTOs.Chat;
using CORE.DTOs.Review;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CORE.DTOs.Rental.RentalHistoryItemDto;

namespace CORE.AutoMapperProfiles
{
    public class AppUserProfile : Profile
    {
        public AppUserProfile()
        {
            CreateMap<AppUser, RegisterDto>()
                .ForMember(dest=> dest.ConfirmPassword, opt=>opt.Ignore())
                .ForMember(dest=> dest.Password, opt=>opt.Ignore())
                .ForMember(dest=> dest.IsCustomer, opt=>opt.Ignore())
                .ForMember(dest=> dest.AggreedTheTerms, opt=>opt.Ignore())
                .ReverseMap();

            CreateMap<AppUser, SummerizedUserDto>()
                .ReverseMap();

            CreateMap<AppUser, UserDetails>();
            CreateMap<AppUser, SummerizedUserWithRateDto>();
        }
    }
}
