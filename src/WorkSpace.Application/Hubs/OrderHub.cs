using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorkSpace.Application.Hubs
{
    public class OrderHub : Hub
    {
        private static readonly Dictionary<string, string> _userConnectionMap = new Dictionary<string, string>();

        public async Task SendNotification(string message, string groupName)
        {
            await Clients.Group(groupName).SendAsync("ReceiveNewsUpdate", message);
        }

        public async Task SendPrivateMessageToUser(string targetUserId, string message)
        {
            if (_userConnectionMap.TryGetValue(targetUserId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceivePrivateMessage", message);
            }
            else
            {
                await Clients.Caller.SendAsync("MessageFailed", $"User {targetUserId} is offline.");
            }
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task RegisterUser(string userId)
        {
            _userConnectionMap[userId] = Context.ConnectionId;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ReceiveConnectionId", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = _userConnectionMap.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (userId != null)
            {
                _userConnectionMap.Remove(userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
