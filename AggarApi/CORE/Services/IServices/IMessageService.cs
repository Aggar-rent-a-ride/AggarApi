using CORE.DTOs;
using CORE.DTOs.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IMessageService
    {
        Task<ResponseDto<GetMessageDto>> CreateMessageAsync(CreateMessageDto messageDto);
    }
}
