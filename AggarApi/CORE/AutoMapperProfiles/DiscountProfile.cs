using AutoMapper;
using CORE.DTOs.Vehicle;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class DiscountProfile: Profile
    {
        public DiscountProfile()
        {
            CreateMap<Discount, DiscountDto>()
                .ForMember(dest => dest.DiscountedPricePerDay, opt => opt.MapFrom(src => decimal.Round(src.Vehicle.PricePerDay - (src.Vehicle.PricePerDay * src.DiscountPercentage / 100), 2, MidpointRounding.AwayFromZero)))
                .ReverseMap();
        }
    }
}
