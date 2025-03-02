using AutoMapper;
using CORE.DTOs.Chat;
using CORE.DTOs.Message;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<CreateMessageDto, Message>()
                .ReverseMap();
            
            CreateMap<Message, GetMessageDto>()
                .ReverseMap();

            CreateMap<Message, ChatMessageDto>()
                .ReverseMap();
        }
    }
}
