using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Message;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public MessageService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
        }
        private string? ValidateCreateMessageDto(CreateMessageDto dto)
        {
            if (dto == null)
                return "Message cannot be null";
            if (dto.SenderId == 0)
                return "Sender cannot be null";
            if (dto.ReceiverId == 0)
                return "Receiver cannot be null";
            if (string.IsNullOrWhiteSpace(dto.Content))
                return "Content cannot be null";
            if (dto.ClientMessageId == null)
                return "ClientMessageId cannot be null";
            return null;
        }
        public async Task<ResponseDto<GetMessageDto>> CreateMessageAsync(CreateMessageDto messageDto)
        {
            if(ValidateCreateMessageDto(messageDto) is string error)
                return new ResponseDto<GetMessageDto>
                {
                    Message = error,
                    StatusCode = StatusCodes.BadRequest,
                    Data = new GetMessageDto { ClientMessageId = messageDto.ClientMessageId }
                };
            var message = _mapper.Map<Message>(messageDto);

            if (messageDto.SenderId == messageDto.ReceiverId)
                return new ResponseDto<GetMessageDto>
                {
                    Message = "Sender and receiver cannot be the same",
                    StatusCode = StatusCodes.BadRequest,
                    Data = new GetMessageDto { ClientMessageId = messageDto.ClientMessageId }
                };

            if(await _userService.CheckAllUsersExist(new List<int> { messageDto.SenderId, messageDto.ReceiverId }) == false)
                return new ResponseDto<GetMessageDto>
                {
                    Message = "users do not exist",
                    StatusCode = StatusCodes.BadRequest,
                    Data = new GetMessageDto { ClientMessageId = messageDto.ClientMessageId }
                };


            await _unitOfWork.Messages.AddOrUpdateAsync(message);
            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
                return new ResponseDto<GetMessageDto>
                {
                    Message = "Failed to send message",
                    StatusCode = StatusCodes.InternalServerError,
                    Data = new GetMessageDto { ClientMessageId = messageDto.ClientMessageId }
                };

            return new ResponseDto<GetMessageDto>
            {
                StatusCode = StatusCodes.Created,
                Data = _mapper.Map<GetMessageDto>(message),
                Message = "Message sent successfully"
            };
        }
    }
}
