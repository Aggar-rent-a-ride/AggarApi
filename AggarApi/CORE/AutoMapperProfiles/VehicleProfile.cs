using AutoMapper;
using CORE.DTOs.Geoapify;
using CORE.DTOs.Vehicle;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class VehicleProfile : Profile
    {
        public VehicleProfile()
        {
            CreateMap<Vehicle, CreateVehicleDto>()
                .ReverseMap();

            CreateMap<Vehicle, UpdateVehicleDto>()
                .ReverseMap();

            CreateMap<Vehicle, GetVehicleDto>()
                .ForMember(dest => dest.VehicleImages,
                    opt => opt.MapFrom(src =>
                        src.VehicleImages != null
                            ? src.VehicleImages.Select(i => i.ImagePath)
                            : new List<string>()))
                .ReverseMap();

        }
    }
}
