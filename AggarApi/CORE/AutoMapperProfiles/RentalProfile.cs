using AutoMapper;
using CORE.DTOs.Geoapify;
using CORE.DTOs.Rental;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class RentalProfile : Profile
    {
        public RentalProfile()
        {
            CreateMap<Rental, GetRentalDto>()
                .ReverseMap();
        }
    }
}
