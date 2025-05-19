using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shofy.Pages.Admin
{
    public class ChatModel : PageModel
    {
        private readonly ShofyContext _context;

        public ChatModel(ShofyContext context)
        {
            _context = context;
        }

        public IList<Order> Orders { get; set; }
        public IList<Message> Messages { get; set; }
        public int? SelectedOrderId { get; set; }

        public async Task<IActionResult> OnGetAsync(int? orderId)
        {
            // Get all orders with their users and messages
            Orders = await _context.Order
                .Include(o => o.User)
                .Include(o => o.Messages)
                .OrderByDescending(o => o.OrderedDate)
                .ToListAsync();

            if (orderId.HasValue)
            {
                SelectedOrderId = orderId;
                Messages = await _context.Message
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .Where(m => m.OrderID == orderId)
                    .OrderBy(m => m.SentDate)
                    .ToListAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetMessagesAsync(int orderId)
        {
            var messages = await _context.Message
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.OrderID == orderId)
                .OrderBy(m => m.SentDate)
                .Select(m => new
                {
                    messageId = m.MessageID,
                    senderId = m.SenderID,
                    senderName = m.Sender.Username,
                    receiverId = m.ReceiverID,
                    receiverName = m.Receiver.Username,
                    message = m.Content,
                    orderId = m.OrderID,
                    sentDate = m.SentDate,
                    isRead = m.IsRead
                })
                .ToListAsync();

            return new JsonResult(messages);
        }
    }
}