using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Message;
using CORE.Services.IServices;
using DATA.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;

        public ChatHub(IMessageService messageService)
        {
            _messageService = messageService;
        }
        public override async Task OnConnectedAsync()
        {
            if(Context.UserIdentifier == null)
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier);

            await base.OnConnectedAsync();
        }
        public async Task SendMessageAsync(CreateMessageDto messageDto)
        {
            if(int.TryParse(Context.UserIdentifier, out int senderId) == false)
            {
                await Clients.Caller.SendAsync(SignalRMethods.ReceiveMessage, new ResponseDto<GetMessageDto> { StatusCode = CORE.Constants.StatusCodes.BadRequest, Message = "Invalid UserIdentifier", Data = new GetMessageDto {ClientMessageId = messageDto.ClientMessageId } });
                return;
            }

            var result = await _messageService.CreateMessageAsync(messageDto, senderId);

            //send to reciever only if saved to db
            if (result.StatusCode == CORE.Constants.StatusCodes.Created) 
                await Clients.Groups(new List<string> { messageDto.ReceiverId.ToString(), Context.UserIdentifier }).SendAsync(SignalRMethods.ReceiveMessage, result);
            
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
