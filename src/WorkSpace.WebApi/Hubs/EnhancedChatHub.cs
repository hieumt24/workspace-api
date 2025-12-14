using System.Collections.Concurrent;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Features.Chat.Commands.MarkMessagesAsRead;
using WorkSpace.Application.Features.Chat.Commands.SendChatMessage;

namespace WorkSpace.WebApi.Hubs;

public class EnhancedChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly ILogger<EnhancedChatHub> _logger;
    
    // Track online users: UserId -> List of ConnectionIds
    private static readonly ConcurrentDictionary<int, HashSet<string>> OnlineUsers = new();
    
    // Track typing users: ThreadId -> List of UserIds
    private static readonly ConcurrentDictionary<int, HashSet<int>> TypingUsers = new();
    
    // Track user's threads: ConnectionId -> List of ThreadIds
    private static readonly ConcurrentDictionary<string, HashSet<int>> UserThreads = new();

    public EnhancedChatHub(IMediator mediator, ILogger<EnhancedChatHub> logger)
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

            _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}", 
                userId, connectionId);

            // Add user to online users
            OnlineUsers.AddOrUpdate(
                userId,
                new HashSet<string> { connectionId },
                (key, existing) =>
                {
                    existing.Add(connectionId);
                    return existing;
                }
            );

            // Initialize user threads tracking
            UserThreads[connectionId] = new HashSet<int>();

            // Notify others that user is online
            await Clients.Others.SendAsync("UserOnline", userId);

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnConnectedAsync");
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = GetCurrentUserId();
            var connectionId = Context.ConnectionId;

            _logger.LogInformation("User {UserId} disconnected with ConnectionId {ConnectionId}", 
                userId, connectionId);

            // Remove connection from online users
            if (OnlineUsers.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                
                // If user has no more connections, remove from online users
                if (connections.Count == 0)
                {
                    OnlineUsers.TryRemove(userId, out _);
                    
                    // Notify others that user is offline
                    await Clients.Others.SendAsync("UserOffline", userId);
                }
            }

            // Leave all thread groups
            if (UserThreads.TryRemove(connectionId, out var threadIds))
            {
                foreach (var threadId in threadIds)
                {
                    // Remove from typing if was typing
                    if (TypingUsers.TryGetValue(threadId, out var typingUserIds))
                    {
                        typingUserIds.Remove(userId);
                        if (typingUserIds.Count == 0)
                        {
                            TypingUsers.TryRemove(threadId, out _);
                        }
                        else
                        {
                            await Clients.Group(GetThreadGroup(threadId))
                                .SendAsync("UserStoppedTyping", threadId, userId);
                        }
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnDisconnectedAsync");
        }
    }

    #endregion

    #region Thread Management

    /// <summary>
    /// Join a chat thread to receive real-time messages
    /// </summary>
    public async Task JoinThread(int threadId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var groupName = GetThreadGroup(threadId);

            _logger.LogInformation("User {UserId} joining thread {ThreadId}", userId, threadId);

            // TODO: Validate user has permission to join this thread
            // This should query database to check if user is participant

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            // Track this thread for this connection
            if (UserThreads.TryGetValue(Context.ConnectionId, out var threads))
            {
                threads.Add(threadId);
            }

            // Get online users in this thread
            var onlineParticipants = GetOnlineUsersInThread(threadId);
            
            await Clients.Caller.SendAsync("JoinedThread", new
            {
                ThreadId = threadId,
                OnlineUsers = onlineParticipants
            });

            // Notify others in thread
            await Clients.OthersInGroup(groupName).SendAsync("UserJoinedThread", threadId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining thread {ThreadId}", threadId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to join thread" });
        }
    }

    /// <summary>
    /// Leave a chat thread
    /// </summary>
    public async Task LeaveThread(int threadId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var groupName = GetThreadGroup(threadId);

            _logger.LogInformation("User {UserId} leaving thread {ThreadId}", userId, threadId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            // Remove from tracked threads
            if (UserThreads.TryGetValue(Context.ConnectionId, out var threads))
            {
                threads.Remove(threadId);
            }

            // Remove from typing
            if (TypingUsers.TryGetValue(threadId, out var typingUserIds))
            {
                typingUserIds.Remove(userId);
            }

            // Notify others
            await Clients.OthersInGroup(groupName).SendAsync("UserLeftThread", threadId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving thread {ThreadId}", threadId);
        }
    }

    #endregion

    #region Messaging

    /// <summary>
    /// Send a message to a thread
    /// </summary>
    public async Task SendMessage(int threadId, string content)
    {
        try
        {
            var userId = GetCurrentUserId();

            _logger.LogInformation("User {UserId} sending message to thread {ThreadId}", 
                userId, threadId);

            // Validate message
            if (string.IsNullOrWhiteSpace(content))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Message cannot be empty" });
                return;
            }

            if (content.Length > 5000)
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Message too long" });
                return;
            }

            // Send command via MediatR
            var command = new SendChatMessageCommand
            {
                SenderId = userId,
                RequestDto = new SendChatMessageRequestDto
                {
                    ThreadId = threadId,
                    Content = content
                }
            };

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                await Clients.Caller.SendAsync("Error", new { Message = result.Message });
                return;
            }

            // Stop typing indicator
            await StopTyping(threadId);

            // Broadcast to thread group
            var groupName = GetThreadGroup(threadId);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", result.Data);

            // Send delivery confirmation to sender
            await Clients.Caller.SendAsync("MessageSent", new
            {
                result.Data.Id,
                result.Data.ThreadId,
                Status = "Delivered"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to thread {ThreadId}", threadId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to send message" });
        }
    }

    /// <summary>
    /// Mark messages as read
    /// </summary>
    public async Task MarkAsRead(int threadId)
    {
        try
        {
            var userId = GetCurrentUserId();

            var command = new MarkMessagesAsReadCommand
            {
                ThreadId = threadId,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                var groupName = GetThreadGroup(threadId);
                await Clients.Group(groupName).SendAsync("MessagesRead", threadId, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read for thread {ThreadId}", threadId);
        }
    }

    #endregion

    #region Typing Indicators

    /// <summary>
    /// Notify that user is typing
    /// </summary>
    public async Task StartTyping(int threadId)
    {
        try
        {
            var userId = GetCurrentUserId();

            TypingUsers.AddOrUpdate(
                threadId,
                new HashSet<int> { userId },
                (key, existing) =>
                {
                    existing.Add(userId);
                    return existing;
                }
            );

            var groupName = GetThreadGroup(threadId);
            await Clients.OthersInGroup(groupName).SendAsync("UserStartedTyping", threadId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in StartTyping for thread {ThreadId}", threadId);
        }
    }

    /// <summary>
    /// Notify that user stopped typing
    /// </summary>
    public async Task StopTyping(int threadId)
    {
        try
        {
            var userId = GetCurrentUserId();

            if (TypingUsers.TryGetValue(threadId, out var typingUserIds))
            {
                typingUserIds.Remove(userId);
                if (typingUserIds.Count == 0)
                {
                    TypingUsers.TryRemove(threadId, out _);
                }
            }

            var groupName = GetThreadGroup(threadId);
            await Clients.OthersInGroup(groupName).SendAsync("UserStoppedTyping", threadId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in StopTyping for thread {ThreadId}", threadId);
        }
    }

    #endregion

    #region Presence

    /// <summary>
    /// Get list of online users
    /// </summary>
    public async Task GetOnlineUsers()
    {
        try
        {
            var onlineUserIds = OnlineUsers.Keys.ToList();
            await Clients.Caller.SendAsync("OnlineUsers", onlineUserIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting online users");
        }
    }

    /// <summary>
    /// Check if specific user is online
    /// </summary>
    public async Task<bool> IsUserOnline(int userId)
    {
        var isOnline = OnlineUsers.ContainsKey(userId);
        await Clients.Caller.SendAsync("UserOnlineStatus", userId, isOnline);
        return isOnline;
    }

    #endregion

    #region Helper Methods

    private static string GetThreadGroup(int threadId) => $"thread-{threadId}";

    private int GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new HubException("User not authenticated");
        }
        return int.Parse(userIdClaim);
    }

    private List<int> GetOnlineUsersInThread(int threadId)
    {
        return OnlineUsers.Keys.ToList();
    }

    #endregion
    
    
}