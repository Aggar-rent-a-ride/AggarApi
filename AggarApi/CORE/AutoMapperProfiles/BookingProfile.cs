using AutoMapper;
using CORE.DTOs.Booking;
using DATA.Models;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<CreateBookingDto, Booking>()
                .ReverseMap();

            CreateMap<BookingDetailsDto, Booking>()
                .ReverseMap();

            CreateMap<GetBookingByRentalIdDto, Booking>()
                .ReverseMap();

            CreateMap<Booking, BookingSummaryDto>()
                .ForMember(dest => dest.BookingStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.VehicleModel, opt => opt.MapFrom(src => src.Vehicle.Model))
                .ForMember(dest => dest.VehicleBrand, opt => opt.MapFrom(src => src.Vehicle.VehicleBrand.Name))
                .ForMember(dest => dest.VehicleType, opt => opt.MapFrom(src => src.Vehicle.VehicleType.Name));
        }
    }
}
