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
        BookingProfile()
        {
            CreateMap<CreateBookingDto, Booking>()
                .ReverseMap();

            CreateMap<BookingDetailsDto, Booking>()
                .ReverseMap();
        }
    }
}
