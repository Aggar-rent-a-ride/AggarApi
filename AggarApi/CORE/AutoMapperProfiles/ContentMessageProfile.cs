using AutoMapper;
using CORE.DTOs.Message;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class ContentMessageProfile : Profile
    {
        public ContentMessageProfile()
        {
            CreateMap<ContentMessage, GetContentMessageDto>()
                .ReverseMap();
            
            CreateMap<CreateContentMessageDto, ContentMessage>()
                .ReverseMap();
        }
    }
}
