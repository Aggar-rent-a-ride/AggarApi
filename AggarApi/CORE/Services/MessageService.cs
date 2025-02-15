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
        private readonly IFileService _fileService;

        public MessageService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _fileService = fileService;
        }
        private async Task<string?> ValidateCreateMessageDto<T>(T dto, int senderId) where T : CreateMessageDto
        {
            if (dto == null)
                return "Message cannot be null";
            if (senderId == 0)
                return "Sender cannot be null";
            if (dto.ReceiverId == 0)
                return "Receiver cannot be null";
            if (dto.ClientMessageId == null)
                return "ClientMessageId cannot be null";
            if (senderId == dto.ReceiverId)
                return "Sender and receiver cannot be the same";
            if (dto is CreateContentMessageDto contentDto)
            {
                if (string.IsNullOrWhiteSpace(contentDto.Content))
                    return "Content cannot be null";
            }
            if (dto is CreateFileMessageDto fileDto)
            {
                if (string.IsNullOrWhiteSpace(fileDto.FilePath))
                    return "file cannot be null";
                if (fileDto.Checksum == null)
                    return "Checksum cannot be null";
                if (fileDto.Checksum == _fileService.HashFile(fileDto.FilePath))
                    return "Checksum does not match";
            }
            if (await _userService.CheckAllUsersExist(new List<int> { senderId, dto.ReceiverId }) == false)
                return "users do not exist";
            return null;
        }
        private async Task<ResponseDto<TGetDto>> BuildMessageDto<TGetDto, TCreateDto, TEntity>(TCreateDto messageDto, int senderId)
            where TGetDto : GetMessageDto
            where TCreateDto : CreateMessageDto
            where TEntity : Message
        {
            if (await ValidateCreateMessageDto(messageDto, senderId) is string error)
            {
                if(messageDto is CreateFileMessageDto fileMessageDto)
                    _fileService.DeleteFile(fileMessageDto?.FilePath);
                return new ResponseDto<TGetDto>
                {
                    Message = error,
                    StatusCode = StatusCodes.BadRequest,
                    Data = new GetMessageDto { ClientMessageId = messageDto.ClientMessageId } as TGetDto
                };
            }

            var message = _mapper.Map<TEntity>(messageDto);
            message.SenderId = senderId;

            await _unitOfWork.Messages.AddOrUpdateAsync(message);
            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
                return new ResponseDto<TGetDto>
                {
                    Message = "Failed to send message",
                    StatusCode = StatusCodes.InternalServerError,
                    Data = new GetMessageDto { ClientMessageId = messageDto.ClientMessageId } as TGetDto
                };

            return new ResponseDto<TGetDto>
            {
                StatusCode = StatusCodes.Created,
                Data = _mapper.Map<TGetDto>(message),
                Message = "message sent successfully"
            };
        }
        public async Task<ResponseDto<TGet>> CreateMessageAsync<TCreate, TGet>(TCreate messageDto, int senderId)
            where TCreate : CreateMessageDto
            where TGet : GetMessageDto
        {
            if (messageDto is CreateContentMessageDto contentMessageDto)
                return await BuildMessageDto<GetContentMessageDto, CreateContentMessageDto, ContentMessage>(contentMessageDto, senderId) as ResponseDto<TGet>;

            else if (messageDto is CreateFileMessageDto fileMessageDto)
                return await BuildMessageDto<GetFileMessageDto, CreateFileMessageDto, FileMessage>(fileMessageDto, senderId) as ResponseDto<TGet>;

            return new ResponseDto<TGet>
            {
                Message = "Invalid message type",
                StatusCode = StatusCodes.BadRequest,
                Data = new GetMessageDto { ClientMessageId = messageDto.ClientMessageId } as TGet
            };
        }
    }
}
