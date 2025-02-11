using CORE.Constants;
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
        private readonly IUserConnectionService _connectionService;
        private readonly IMessageService _messageService;

        public ChatHub(IUserConnectionService connectionService, IMessageService messageService)
        {
            _connectionService = connectionService;
            _messageService = messageService;
        }
        public override async Task OnConnectedAsync()
        {
            if (int.TryParse(Context.UserIdentifier, out int userId) == false)
                return;

            var connection = await _connectionService.CreateUserConnectionAsync(userId, Context.ConnectionId);
            
            if (connection == null)
                return;

            await base.OnConnectedAsync();
        }
        public async Task SendMessageAsync(CreateMessageDto messageDto)
        {
            var result = await _messageService.CreateMessageAsync(messageDto);

            //either if saved to db or not, cuz if it wasn't saved the response dto.status code will be 500
            await Clients.Caller.SendAsync(SignalRMethods.ReceiveMessage, result);

            //send to reciever only if saved to db
            if (result.StatusCode == CORE.Constants.StatusCodes.Created) 
            {
                var receiverConnections = await _connectionService.GetAllUserConnectionsAsync(messageDto.ReceiverId);

                if (receiverConnections != null && receiverConnections.Count != 0)
                    await Clients.Clients(receiverConnections.Select(c => c.ConnectionId)).SendAsync(SignalRMethods.ReceiveMessage, result);
            }
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connection = await _connectionService.DisconnectAsync(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
