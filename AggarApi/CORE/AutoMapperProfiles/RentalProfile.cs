using AutoMapper;
using CORE.DTOs.Geoapify;
using CORE.DTOs.Rental;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class RentalProfile : Profile
    {
        public RentalProfile()
        {
            CreateMap<Rental, GetRentalDto>()
                .ForMember(dest => dest.RentalStatus, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();

            CreateMap<Rental, GetRentalsByUserIdDto>()
                .ForMember(dest => dest.Booking, opt => opt.MapFrom(src => src.Booking));

            CreateMap<Booking, GetRentalsByUserIdDto.GetRentalsByUserIdDtoBooking>()
                .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle));

            CreateMap<Vehicle, GetRentalsByUserIdDto.GetRentalsByUserIdDtoBooking.GetRentalsByUserIdDtoVehicle>();

            CreateMap<Rental, GetRentalsByVehicleIdDto>();

        }
    }
}
