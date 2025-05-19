using Microsoft.AspNetCore.SignalR;
using Shofy.Data;
using Shofy.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Shofy.Helpers;

namespace Shofy.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ShofyContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatHub(ShofyContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task JoinOrderGroup(int orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
        }

        public async Task LeaveOrderGroup(int orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
        }

        public async Task SendMessage(string message, int orderId, int receiverId)
        {
            var userId = _httpContextAccessor.HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                throw new HubException("User not authenticated");
            }

            var user = await _context.User.FindAsync(userId.Value);
            var order = await _context.Order.FindAsync(orderId);
            var receiver = await _context.User.FindAsync(receiverId);

            if (user == null || order == null || receiver == null)
            {
                throw new HubException("Invalid user, order, or receiver");
            }

            // Save message to database
            var newMessage = new Message
            {
                SenderID = userId.Value,
                ReceiverID = receiverId,
                Content = message,
                OrderID = orderId,
                SentDate = DateTime.Now,
                IsRead = false,
                GroupName = $"order-{orderId}"
            };

            _context.Message.Add(newMessage);
            await _context.SaveChangesAsync();

            // Send message to group
            await Clients.Group($"order-{orderId}").SendAsync("ReceiveMessage", new
            {
                messageId = newMessage.MessageID,
                senderId = userId.Value,
                senderName = user.Username,
                message = message,
                orderId = orderId,
                sentDate = DateTime.Now.ToString("o"), // Chuẩn hóa định dạng thời gian
                isRead = false
            });

            // Update order list for admin
            await Clients.All.SendAsync("UpdateOrderList");
        }

        public async Task MarkMessageAsRead(int messageId)
        {
            var message = await _context.Message.FindAsync(messageId);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
                await Clients.Group(message.GroupName).SendAsync("MessageRead", messageId);
            }
        }
    }
}