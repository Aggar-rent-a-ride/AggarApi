using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.AppUser;
using CORE.DTOs.Chat;
using CORE.DTOs.Message;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Constants.Enums;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CORE.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IFileService _fileService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(IUnitOfWork unitOfWork, 
            IMapper mapper, 
            IUserService userService, 
            IFileService fileService,
            ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _fileService = fileService;
            _logger = logger;
        }
        private async Task<string?> ValidateSenderAndReceiver(int senderId, int receiverId)
        {
            if (senderId == 0)
                return "Sender cannot be null";
            if (receiverId == 0)
                return "Receiver cannot be null";
            if (senderId == receiverId)
                return "Sender and receiver cannot be the same";
            if (await _userService.CheckAllUsersExist(new List<int> { senderId, receiverId }) == false)
                return "Users do not exist";
            return null;
        }
        private async Task<string?> ValidateCreateMessageDto<T>(T dto, int senderId) where T : CreateMessageDto
        {
            if (dto == null)
                return "Message cannot be null";
            if (dto.ClientMessageId == null)
                return "ClientMessageId cannot be null";
            if (dto is CreateContentMessageDto contentDto)
            {
                if (string.IsNullOrWhiteSpace(contentDto.Content))
                    return "Content cannot be null";
            }
            if (dto is CreateFileMessageDto fileDto)
            {
                if (string.IsNullOrWhiteSpace(fileDto.FilePath))
                    return "File cannot be null";
                if (fileDto.Checksum == null)
                    return "Checksum cannot be null";
                if (fileDto.Checksum != _fileService.HashFile(fileDto.FilePath))
                    return "Checksum does not match";
            }
            if(await ValidateSenderAndReceiver(senderId, dto.ReceiverId) is string error)
                return error;
            return null;
        }
        private async Task<ResponseDto<TGetDto>> BuildMessageDto<TGetDto, TCreateDto, TEntity>(TCreateDto messageDto, int senderId)
            where TGetDto : GetMessageDto
            where TCreateDto : CreateMessageDto
            where TEntity : Message
        {
            _logger.LogInformation("Building message DTO for sender ID {SenderId}. Message type: {MessageType}", senderId, typeof(TCreateDto).Name);

            if (await ValidateCreateMessageDto(messageDto, senderId) is string error)
            {
                _logger.LogWarning("Message validation failed for sender ID {SenderId}. Error: {Error}", senderId, error);

                if (messageDto is CreateFileMessageDto fileMessageDto)
                {
                    _logger.LogInformation("Deleting file due to validation failure. File path: {FilePath}", fileMessageDto.FilePath);
                    _fileService.DeleteFile(fileMessageDto?.FilePath);
                }
                return new ResponseDto<TGetDto>
                {
                    Message = error,
                    StatusCode = StatusCodes.BadRequest,
                    Data = new GetMessageDto { ClientMessageId = messageDto.ClientMessageId } as TGetDto
                };
            }
            _logger.LogInformation("Message validation succeeded. Mapping DTO to entity.");

            var message = _mapper.Map<TEntity>(messageDto);
            message.SenderId = senderId;

            _logger.LogInformation("Saving message to database.");
            await _unitOfWork.Chat.AddOrUpdateAsync(message);
            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
            {
                _logger.LogError("Failed to save message for sender ID {SenderId}.", senderId);
                return new ResponseDto<TGetDto>
                {
                    Message = "Failed to send message",
                    StatusCode = StatusCodes.InternalServerError,
                    Data = new GetMessageDto { ClientMessageId = messageDto.ClientMessageId } as TGetDto
                };
            }

            _logger.LogInformation("Message sent successfully for sender ID {SenderId}.", senderId);
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
            _logger.LogInformation("Creating message from sender ID {SenderId}. Message type: {MessageType}", senderId, typeof(TCreate).Name);

            if (messageDto is CreateContentMessageDto contentMessageDto)
            {
                _logger.LogInformation("Processing content message.");
                return await BuildMessageDto<GetContentMessageDto, CreateContentMessageDto, ContentMessage>(contentMessageDto, senderId) as ResponseDto<TGet>;
            }

            else if (messageDto is CreateFileMessageDto fileMessageDto)
            {
                _logger.LogInformation("Processing file message.");
                return await BuildMessageDto<GetFileMessageDto, CreateFileMessageDto, FileMessage>(fileMessageDto, senderId) as ResponseDto<TGet>;
            }

            _logger.LogWarning("Invalid message type received from sender ID {SenderId}.", senderId);
            return new ResponseDto<TGet>
            {
                Message = "Invalid message type",
                StatusCode = StatusCodes.BadRequest,
                Data = new GetMessageDto { ClientMessageId = messageDto?.ClientMessageId } as TGet
            };
        }
        public async Task<ResponseDto<ArrayList>> GetMessagesAsync(int userId1, int userId2, DateTime dateTime, int pageSize, DateFilter dateFilter, int maxPageSize = 100)
        {
            _logger.LogInformation("Retrieving messages between user {UserId1} and user {UserId2} with page size {PageSize} and date filter {DateFilter}.",
                userId1, userId2, pageSize, dateFilter);

            if (pageSize <= 0)
            {
                _logger.LogWarning("Invalid page size: {PageSize}.", pageSize);
                return new ResponseDto<ArrayList>
                {
                    Message = "Invalid page size",
                    StatusCode = StatusCodes.BadRequest,
                };
            }

            if (pageSize > maxPageSize)
            {
                _logger.LogWarning("Page size exceeds maximum limit: {PageSize}.", pageSize);
                return new ResponseDto<ArrayList>
                {
                    Message = $"Page size must be less than or equal to {maxPageSize}",
                    StatusCode = StatusCodes.BadRequest,
                };
            }

            if (await ValidateSenderAndReceiver(userId1, userId2) is string senderReceiverErorr)
            {
                _logger.LogWarning("Sender and receiver validation failed: {Error}", senderReceiverErorr);
                return new ResponseDto<ArrayList>
                {
                    Message = senderReceiverErorr,
                    StatusCode = StatusCodes.BadRequest,
                };
            }

            _logger.LogInformation("Fetching messages from database.");
            var messages = dateFilter == DateFilter.Before ?
                await _unitOfWork.Chat.FindAsync(
                m => ((m.SenderId == userId1 && m.ReceiverId == userId2) || (m.SenderId == userId2 && m.ReceiverId == userId1)) && m.SentAt < dateTime,
                1, //don't skip any messages
                pageSize,
                sortingExpression: x => x.SentAt,
                sortingDirection: OrderBy.Descending) :
                await _unitOfWork.Chat.FindAsync(
                m => ((m.SenderId == userId1 && m.ReceiverId == userId2) || (m.SenderId == userId2 && m.ReceiverId == userId1)) && m.SentAt > dateTime,
                1, //don't skip any messages
                pageSize,
                sortingExpression: x => x.SentAt,
                sortingDirection: OrderBy.Descending);

            messages = messages.Reverse();

            _logger.LogInformation("Mapping retrieved messages to DTOs.");
            var result = new ArrayList();

            foreach (var msg in messages)
            {
                if(msg is ContentMessage contentMessage)
                    result.Add(_mapper.Map<GetContentMessageDto>(contentMessage));
                else if (msg is FileMessage fileMessage)
                    result.Add(_mapper.Map<GetFileMessageDto>(fileMessage));
            }

            _logger.LogInformation("Successfully retrieved {MessageCount} messages between user {UserId1} and user {UserId2}.", result.Count, userId1, userId2);
            return new ResponseDto<ArrayList>
            {
                StatusCode = StatusCodes.OK,
                Data = result,
                Message = "Messages retrieved successfully"
            };
        }
        public async Task<ResponseDto<ArrayList>> GetChatAsync(int authUserId, int pageNo, int pageSize, int maxPageSize = 100)
        {
            _logger.LogInformation("Fetching chat for user {AuthUserId} with page {PageNo} and page size {PageSize}.", authUserId, pageNo, pageSize);

            if (PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string paginationError)
            {
                _logger.LogWarning("Pagination validation failed: {Error}", paginationError);
                return new ResponseDto<ArrayList>
                {
                    Message = paginationError,
                    StatusCode = StatusCodes.BadRequest,
                };
            }

            if(authUserId == 0 || await _userService.CheckAnyAsync(authUserId) == false)
            {
                _logger.LogWarning("User {AuthUserId} does not exist.", authUserId);
                return new ResponseDto<ArrayList>
                {
                    Message = "User does not exist",
                    StatusCode = StatusCodes.Unauthorized,
                };
            }

            _logger.LogInformation("Retrieving latest chat messages for user {AuthUserId}.", authUserId);
            var chatItems = await _unitOfWork.Chat.GetLatestChatMessagesAsync(authUserId, pageNo, pageSize);

            _logger.LogInformation("Mapping chat messages to DTOs.");
            var chatList = new ArrayList();
            foreach (var item in chatItems)
            {
                var user = item.SenderId == authUserId ? _mapper.Map<SummerizedUserDto>(item.Receiver) : _mapper.Map<SummerizedUserDto>(item.Sender);
                var lastUnseenMessageIds = await _unitOfWork.Chat.GetLatestUnseenMessagesIds(authUserId, user.Id);
                if (item is ContentMessage contentMessage)
                    chatList.Add(new ChatItemDto<ChatContentMessageDto>
                    {
                        UnseenMessageIds = lastUnseenMessageIds,
                        User = user,
                        LastMessage = _mapper.Map<ChatContentMessageDto>(contentMessage)
                    });
                else if (item is FileMessage fileMessage)
                    chatList.Add(new ChatItemDto<ChatFileMessageDto>
                    {
                        UnseenMessageIds = lastUnseenMessageIds,
                        User = user,
                        LastMessage = _mapper.Map<ChatFileMessageDto>(fileMessage)
                    });
            }

            _logger.LogInformation("Successfully retrieved {ChatCount} chat items for user {AuthUserId}.", chatList.Count, authUserId);
            return new ResponseDto<ArrayList>
            {
                StatusCode = StatusCodes.OK,
                Data = chatList,
                Message = "Chat retrieved successfully"
            };
        }
        public async Task<ResponseDto<object>> AcknowledgeMessagesAsync(int authUserId, HashSet<int> messageIds)
        {
            _logger.LogInformation("Acknowledging messages for user {AuthUserId} with message IDs: {MessageIds}", authUserId, string.Join(", ", messageIds));

            var messages = (await _unitOfWork.Chat.GetAllAsync(m => m.ReceiverId == authUserId && messageIds.Contains(m.Id) && m.IsSeen == false)).ToList();

            if (messages == null || messages.Count == 0)
            {
                _logger.LogWarning("No unseen messages found for user {AuthUserId} with the given message IDs.", authUserId);
                return new ResponseDto<object>
                {
                    Message = "No messages found to acknowledge",
                    StatusCode = StatusCodes.BadRequest,
                };
            }

            _logger.LogInformation("Marking {MessageCount} messages as seen for user {AuthUserId}.", messages.Count, authUserId);
            messages.ForEach(m => m.IsSeen = true);

            var changes = await _unitOfWork.CommitAsync();
            
            if(changes == 0)
            {
                _logger.LogError("Failed to acknowledge messages for user {AuthUserId}.", authUserId);
                return new ResponseDto<object>
                {
                    Message = "Failed to acknowledge messages",
                    StatusCode = StatusCodes.InternalServerError,
                };
            }

            _logger.LogInformation("Successfully acknowledged {MessageCount} messages for user {AuthUserId}.", messages.Count, authUserId);
            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.OK,
                Message = "Messages acknowledged successfully"
            };
        }
        public async Task<ResponseDto<ArrayList>> FilterMessagesAsync(MessageFilterDto filter, int authUserId, int maxPageSize = 100)
        {
            _logger.LogInformation("Filtering messages for user {AuthUserId} with filter: {@Filter}", authUserId, filter);

            if (PaginationHelpers.ValidatePaging(filter.PageNo, filter.PageSize, maxPageSize) is string paginationError)
            {
                _logger.LogWarning("Invalid pagination parameters: {PaginationError}", paginationError);
                return new ResponseDto<ArrayList>
                {
                    Message = paginationError,
                    StatusCode = StatusCodes.BadRequest,
                };
            }

            if (authUserId == 0 || filter.UserId == 0 || await _userService.CheckAllUsersExist(new List<int> {authUserId, filter.UserId}) == false)
            {
                _logger.LogWarning("User validation failed. AuthUserId: {AuthUserId}, FilterUserId: {FilterUserId}", authUserId, filter.UserId);
                return new ResponseDto<ArrayList>
                {
                    Message = "User does not exist",
                    StatusCode = StatusCodes.BadRequest,
                };
            }

            _logger.LogInformation("Fetching filtered messages between users {AuthUserId} and {FilterUserId}.", authUserId, filter.UserId);

            var messages = await _unitOfWork.Chat.FilterMessagesAsync(authUserId,
                filter.UserId,
                filter.SearchQuery,
                filter.Date,
                filter.PageNo,
                filter.PageSize,
                sortingExpression: m => m.Id,
                sortingDirection: OrderBy.Descending);

            var result = new ArrayList();

            foreach (var msg in messages)
            {
                if (msg is ContentMessage content)
                    result.Add(_mapper.Map<GetContentMessageDto>(content));
                else if (msg is FileMessage file)
                    result.Add(_mapper.Map<GetFileMessageDto>(file));
            }

            _logger.LogInformation("{MessageCount} messages retrieved successfully for user {AuthUserId}.", result.Count, authUserId);

            return new ResponseDto<ArrayList>
            {
                StatusCode = StatusCodes.OK,
                Data = result,
                Message = "Messages retrieved successfully"
            };
        }
    }
}
