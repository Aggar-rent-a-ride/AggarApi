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
    public class FileMessageProfile : Profile
    {
        public FileMessageProfile()
        {
            CreateMap<FileMessage, GetFileMessageDto>()
                .ReverseMap();

            CreateMap<CreateFileMessageDto, FileMessage>()
                .ReverseMap();
        }
    }
}
