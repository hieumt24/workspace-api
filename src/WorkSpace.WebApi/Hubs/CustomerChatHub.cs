using System.Collections.Concurrent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Features.CustomerChat.Commands.OwnerReplyToCustomer;
using WorkSpace.Application.Features.CustomerChat.Commands.SendCustomerMessage;
using WorkSpace.Application.Features.CustomerChat.Queries.GetCustomerChatMessages;

namespace WorkSpace.WebApi.Hubs;

public class CustomerChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly ILogger<CustomerChatHub> _logger;

    private static readonly ConcurrentDictionary<string, HashSet<string>> OnlineCustomers = new();
    private static readonly ConcurrentDictionary<string, HashSet<string>> OnlineOwners = new();

    private static readonly ConcurrentDictionary<string, string> TypingUsers = new();

    public CustomerChatHub(IMediator mediator, ILogger<CustomerChatHub> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    #region Connection Management

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var connectionId = Context.ConnectionId;

            _logger.LogInformation(
                "CustomerChat: User {UserId} connected with ConnectionId {ConnectionId}",
                userId, connectionId);

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CustomerChat OnConnectedAsync");
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var connectionId = Context.ConnectionId;

            // Remove from online customers
            foreach (var kvp in OnlineCustomers)
            {
                if (kvp.Value.Remove(connectionId))
                {
                    if (kvp.Value.Count == 0)
                    {
                        OnlineCustomers.TryRemove(kvp.Key, out _);
                        // Notify session that customer went offline
                        await Clients.Group(GetSessionGroup(kvp.Key))
                            .SendAsync("CustomerOffline", kvp.Key);
                    }
                    break;
                }
            }

            // Remove from online owners
            foreach (var kvp in OnlineOwners)
            {
                if (kvp.Value.Remove(connectionId))
                {
                    if (kvp.Value.Count == 0)
                    {
                        OnlineOwners.TryRemove(kvp.Key, out _);
                        // Notify session that owner went offline
                        await Clients.Group(GetSessionGroup(kvp.Key))
                            .SendAsync("OwnerOffline", kvp.Key);
                    }
                    break;
                }
            }

            _logger.LogInformation("CustomerChat: ConnectionId {ConnectionId} disconnected", connectionId);

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CustomerChat OnDisconnectedAsync");
        }
    }

    #endregion

    #region Session Management

    /// <summary>
    /// Customer joins a chat session
    /// </summary>
    // [Authorize(Roles = "Customer")]
    public async Task JoinSession(string sessionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var groupName = GetSessionGroup(sessionId);

            _logger.LogInformation(
                "CustomerChat: Customer {UserId} joining session {SessionId}",
                userId, sessionId);

            // Add to group
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Track online customer
            OnlineCustomers.AddOrUpdate(
                sessionId,
                new HashSet<string> { Context.ConnectionId },
                (key, existing) =>
                {
                    existing.Add(Context.ConnectionId);
                    return existing;
                });

            // Get chat history
            var query = new GetCustomerChatMessagesQuery { SessionId = sessionId };
            var messages = await _mediator.Send(query);

            // Send to caller
            await Clients.Caller.SendAsync("SessionJoined", new
            {
                SessionId = sessionId,
                Messages = messages
            });

            // Notify owner if online
            await Clients.OthersInGroup(groupName)
                .SendAsync("CustomerJoined", sessionId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining customer session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to join session" });
        }
    }

    /// <summary>
    /// Owner joins a chat session
    /// </summary>
    // [Authorize(Roles = "Owner,Admin")]
    public async Task JoinSessionAsOwner(string sessionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var groupName = GetSessionGroup(sessionId);

            _logger.LogInformation(
                "CustomerChat: Owner {UserId} joining session {SessionId}",
                userId, sessionId);

            // Add to group
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Track online owner
            OnlineOwners.AddOrUpdate(
                sessionId,
                new HashSet<string> { Context.ConnectionId },
                (key, existing) =>
                {
                    existing.Add(Context.ConnectionId);
                    return existing;
                });

            // Get chat history
            var query = new GetCustomerChatMessagesQuery { SessionId = sessionId };
            var messages = await _mediator.Send(query);

            // Send to caller
            await Clients.Caller.SendAsync("SessionJoined", new
            {
                SessionId = sessionId,
                Messages = messages
            });

            // Notify customer if online
            await Clients.OthersInGroup(groupName)
                .SendAsync("OwnerJoined", sessionId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error owner joining session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to join session" });
        }
    }

    /// <summary>
    /// Leave a chat session
    /// </summary>
    public async Task LeaveSession(string sessionId)
    {
        try
        {
            var groupName = GetSessionGroup(sessionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("CustomerChat: User left session {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving session {SessionId}", sessionId);
        }
    }

    #endregion

    #region Messaging

    /// <summary>
    /// Customer sends a message
    /// </summary>
    // [Authorize(Roles = "Customer")]
    public async Task SendMessage(string sessionId, string message)
    {
        try
        {
            _logger.LogInformation(
                "CustomerChat: Customer sending message to session {SessionId}",
                sessionId);

            // Validate
            if (string.IsNullOrWhiteSpace(message))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Message cannot be empty" });
                return;
            }

            if (message.Length > 5000)
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Message too long" });
                return;
            }

            // Send via MediatR
            var command = new SendCustomerMessageCommand
            {
                RequestDto = new SendCustomerMessageRequestDto
                {
                    SessionId = sessionId,
                    Message = message
                }
            };

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                await Clients.Caller.SendAsync("Error", new { Message = result.Message });
                return;
            }

            // Stop typing
            await StopTyping(sessionId);

            // Broadcast to session group
            var groupName = GetSessionGroup(sessionId);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", result.Data);

            // Notify owner via all their sessions (if they're not in this specific session)
            await NotifyOwnerNewMessage(sessionId, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error customer sending message to session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to send message" });
        }
    }

    /// <summary>
    /// Owner replies to customer
    /// </summary>
    // [Authorize(Roles = "Owner,Admin")]
    public async Task ReplyToCustomer(string sessionId, string message)
    {
        try
        {
            var ownerId = GetCurrentUserId();

            _logger.LogInformation(
                "CustomerChat: Owner {OwnerId} replying to session {SessionId}",
                ownerId, sessionId);

            // Validate
            if (string.IsNullOrWhiteSpace(message))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Message cannot be empty" });
                return;
            }

            if (message.Length > 5000)
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Message too long" });
                return;
            }

            // Send via MediatR
            var command = new OwnerReplyToCustomerCommand
            {
                SessionId = sessionId,
                Message = message,
                OwnerUserId = ownerId
            };

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                await Clients.Caller.SendAsync("Error", new { Message = result.Message });
                return;
            }

            // Stop typing
            await StopTyping(sessionId);

            // Broadcast to session group
            var groupName = GetSessionGroup(sessionId);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error owner replying to session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to send message" });
        }
    }

    #endregion

    #region Typing Indicators

    /// <summary>
    /// User started typing
    /// </summary>
    public async Task StartTyping(string sessionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            TypingUsers.AddOrUpdate(sessionId, userName, (key, existing) => userName);

            var groupName = GetSessionGroup(sessionId);
            await Clients.OthersInGroup(groupName)
                .SendAsync("UserStartedTyping", sessionId, userId, userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in StartTyping for session {SessionId}", sessionId);
        }
    }

    /// <summary>
    /// User stopped typing
    /// </summary>
    public async Task StopTyping(string sessionId)
    {
        try
        {
            var userId = GetCurrentUserId();

            TypingUsers.TryRemove(sessionId, out _);

            var groupName = GetSessionGroup(sessionId);
            await Clients.OthersInGroup(groupName)
                .SendAsync("UserStoppedTyping", sessionId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in StopTyping for session {SessionId}", sessionId);
        }
    }

    #endregion

    #region Presence

    /// <summary>
    /// Check if customer is online in session
    /// </summary>
    public async Task<bool> IsCustomerOnline(string sessionId)
    {
        var isOnline = OnlineCustomers.ContainsKey(sessionId);
        await Clients.Caller.SendAsync("CustomerOnlineStatus", sessionId, isOnline);
        return isOnline;
    }

    /// <summary>
    /// Check if owner is online in session
    /// </summary>
    public async Task<bool> IsOwnerOnline(string sessionId)
    {
        var isOnline = OnlineOwners.ContainsKey(sessionId);
        await Clients.Caller.SendAsync("OwnerOnlineStatus", sessionId, isOnline);
        return isOnline;
    }

    #endregion

    #region Helper Methods

    private static string GetSessionGroup(string sessionId) => $"customer-chat-{sessionId}";

    private int GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new HubException("User not authenticated");
        }
        return int.Parse(userIdClaim);
    }

    private string GetCurrentUserName()
    {
        return Context.User?.Identity?.Name ?? "Unknown";
    }

    /// <summary>
    /// Notify owner about new message from customer (global notification)
    /// </summary>
    private async Task NotifyOwnerNewMessage(string sessionId, CustomerChatMessageDto message)
    {
        try
        {
            // Send to all owner connections (not just in this session)
            // This allows owners to see notifications in their dashboard
            await Clients.Group("owners").SendAsync("NewCustomerMessage", new
            {
                SessionId = sessionId,
                Message = message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying owner about new message");
        }
    }

    /// <summary>
    /// Join owners group for global notifications
    /// </summary>
    // [Authorize(Roles = "Owner,Admin")]
    public async Task JoinOwnersGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "owners");
        _logger.LogInformation("Owner joined global owners group");
    }

    #endregion
}