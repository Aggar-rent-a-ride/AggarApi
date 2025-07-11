﻿using AutoMapper;
using CORE.DTOs.Geoapify;
using CORE.DTOs.Vehicle;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CORE.DTOs.Rental.RentalHistoryItemDto;

namespace CORE.AutoMapperProfiles
{
    public class VehicleProfile : Profile
    {
        public VehicleProfile()
        {
            CreateMap<Vehicle, CreateVehicleDto>()
                .ForMember(dest => dest.PricePerDay, opt => opt.MapFrom(src => decimal.Round(src.PricePerDay, 2, MidpointRounding.AwayFromZero)))
                .ReverseMap()
                .ForMember(dest => dest.PricePerDay, opt => opt.MapFrom(src => decimal.Round(src.PricePerDay, 2, MidpointRounding.AwayFromZero)));

            CreateMap<Vehicle, UpdateVehicleDto>()
                .ForMember(dest => dest.PricePerDay, opt => opt.MapFrom(src => decimal.Round(src.PricePerDay, 2, MidpointRounding.AwayFromZero)))
                .ReverseMap()
                .ForMember(dest => dest.PricePerDay, opt => opt.MapFrom(src => decimal.Round(src.PricePerDay, 2, MidpointRounding.AwayFromZero)));

            CreateMap<Vehicle, GetVehicleDto>()
                .ForMember(dest => dest.PricePerDay, opt=>opt.MapFrom(src=>decimal.Round(src.PricePerDay, 2, MidpointRounding.AwayFromZero)))
                .ForMember(dest => dest.VehicleImages,
                    opt => opt.MapFrom(src =>
                        src.VehicleImages != null
                            ? src.VehicleImages.Select(i => i.ImagePath)
                            : new List<string>()))
                .ReverseMap()
                .ForMember(dest => dest.PricePerDay, opt => opt.MapFrom(src => decimal.Round(src.PricePerDay, 2, MidpointRounding.AwayFromZero)));

            CreateMap<Vehicle, GetVehicleSummaryDto>()
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.VehicleBrand.Name))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.VehicleType.Name));

            CreateMap<Vehicle, RenterVehiclesDto>()
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.VehicleBrand.Name))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.VehicleType.Name));

            CreateMap<Vehicle, VehicleDetails>();
        }
    }
}
