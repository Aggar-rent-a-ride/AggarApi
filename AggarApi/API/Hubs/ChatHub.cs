using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Message;
using CORE.DTOs.Paths;
using CORE.Services.IServices;
using DATA.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;

namespace API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IFileService _fileService;
        private readonly IOptions<Paths> _paths;
        private readonly IFileCacheService _fileCacheService;

        public ChatHub(IChatService chatService, 
            IFileService fileService, 
            IOptions<Paths> paths, 
            IFileCacheService fileCacheService)
        {
            _chatService = chatService;
            _fileService = fileService;
            _paths = paths;
            _fileCacheService = fileCacheService;
        }
        public override async Task OnConnectedAsync()
        {
            if(Context.UserIdentifier == null)
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier);

            await base.OnConnectedAsync();
        }

        public async Task SendMessageAsync(CreateContentMessageDto messageDto)
        {
            if(int.TryParse(Context.UserIdentifier, out int senderId) == false)
            {
                await Clients.Group(Context.UserIdentifier).SendAsync(SignalRMethods.ReceiveMessage, new ResponseDto<GetMessageDto> { StatusCode = CORE.Constants.StatusCodes.BadRequest, Message = "Invalid UserIdentifier", Data = new GetMessageDto {ClientMessageId = messageDto.ClientMessageId } });
                return;
            }

            var result = await _chatService.CreateMessageAsync<CreateContentMessageDto, GetContentMessageDto>(messageDto, senderId);

            //send to reciever only if saved to db
            if (result.StatusCode == CORE.Constants.StatusCodes.Created) 
                await Clients.Groups(new List<string> { messageDto.ReceiverId.ToString(), Context.UserIdentifier }).SendAsync(SignalRMethods.ReceiveMessage, result);
            
            //either if saved to db or not, cuz if it wasn't saved the response dto.status code will be 500
            else
                await Clients.Group(Context.UserIdentifier).SendAsync(SignalRMethods.ReceiveMessage, result);
        }
        public async Task InitiateUploadingAsync(FileInfoRequestDto dto)
        {
            if (int.TryParse(Context.UserIdentifier, out int senderId) == false)
            {
                await Clients.Group(Context.UserIdentifier).SendAsync(SignalRMethods.UploadInitiationResponse, new ResponseDto<FileInfoResponseDto> { StatusCode = CORE.Constants.StatusCodes.BadRequest, Message = "Invalid UserIdentifier", Data = new FileInfoResponseDto { ClientMessageId = dto.ClientMessageId } });
                return;
            }
            //add image extensions
            var allowedExtensions = AllowedExtensions.ImageExtensions
                .Concat(AllowedExtensions.FileExtensions)
                .ToList();


            var filePath = await _fileService.CreateFile(_paths.Value.MessageFiles, dto.Name, dto.Extension, allowedExtensions);
            
            if(filePath == null)
            {
                await Clients.Group(Context.UserIdentifier).SendAsync(SignalRMethods.UploadInitiationResponse, new ResponseDto<FileInfoResponseDto> { StatusCode = CORE.Constants.StatusCodes.InternalServerError, Message = "Failed to create file", Data = new FileInfoResponseDto { ClientMessageId = dto.ClientMessageId } });
                return;
            }

            await _fileCacheService.AddAsync(filePath);

            await Clients.Group(Context.UserIdentifier).SendAsync(SignalRMethods.UploadInitiationResponse, new ResponseDto<FileInfoResponseDto> { StatusCode = CORE.Constants.StatusCodes.OK, Message = "File initiated", Data = new FileInfoResponseDto { ClientMessageId = dto.ClientMessageId, FilePath = filePath } });
        }
        public async Task UploadAsync(CreateFileMessageDto dto)
        {
            if (int.TryParse(Context.UserIdentifier, out int senderId) == false)
            {
                await Clients.Group(Context.UserIdentifier).SendAsync(SignalRMethods.ReceiveUploadingProgress, new ResponseDto<FileUploadProgressDto> { StatusCode = CORE.Constants.StatusCodes.BadRequest, Message = "Invalid UserIdentifier", Data = new FileUploadProgressDto { ClientMessageId = dto.ClientMessageId } });
                return;
            }
            else if (dto.ClientMessageId == null)
            {
                await Clients.Group(Context.UserIdentifier).SendAsync(SignalRMethods.ReceiveUploadingProgress, new ResponseDto<FileUploadProgressDto> { StatusCode = CORE.Constants.StatusCodes.BadRequest, Message = "Invalid ClientMessageId", Data = new FileUploadProgressDto { ClientMessageId = dto.ClientMessageId } });
                return;
            }

            var result = await _fileService.UploadFileAsync(dto.FilePath, dto.BytesBase64);
            result.Data.ClientMessageId = dto.ClientMessageId;

            await Clients.Group(Context.UserIdentifier).SendAsync(SignalRMethods.ReceiveUploadingProgress, result);
        }
        public async Task FinishUploadingAsync(CreateFileMessageDto dto) //it's like sending message exactly
        {
            if (int.TryParse(Context.UserIdentifier, out int senderId) == false)
            {
                await Clients.Group(Context.UserIdentifier).SendAsync(SignalRMethods.ReceiveMessage, new ResponseDto<GetFileMessageDto> { StatusCode = CORE.Constants.StatusCodes.BadRequest, Message = "Invalid UserIdentifier", Data = new GetFileMessageDto { ClientMessageId = dto.ClientMessageId } });
                return;
            }

            var result = await _chatService.CreateMessageAsync<CreateFileMessageDto, GetFileMessageDto>(dto, int.Parse(Context.UserIdentifier));

            //send to reciever only if saved to db
            if (result.StatusCode == CORE.Constants.StatusCodes.Created)
                await Clients.Groups(new List<string> { dto.ReceiverId.ToString(), Context.UserIdentifier }).SendAsync(SignalRMethods.ReceiveMessage, result);

            //either if saved to db or not, cuz if it wasn't saved the response dto.status code will be 500
            else
                await Clients.Group(Context.UserIdentifier).SendAsync(SignalRMethods.ReceiveMessage, result);
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if(Context.UserIdentifier != null)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.UserIdentifier);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
