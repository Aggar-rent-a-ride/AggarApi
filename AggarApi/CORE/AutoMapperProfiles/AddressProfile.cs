using AutoMapper;
using CORE.DTOs.Auth;
using CORE.DTOs.Geoapify;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class AddressProfile : Profile
    {
        public AddressProfile()
        {
            CreateMap<Address, GeoapifyAddressResponse>()
                .ReverseMap();
        }
    }
}
