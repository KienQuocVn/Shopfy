using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Models;
using System.Threading.Tasks;
using Shofy.Data;

namespace Shofy.Pages.Client
{
    public class SendMessageModel : PageModel
    {
        private readonly ShofyContext _context;

        public SendMessageModel(ShofyContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnPostAsync(int orderId, int senderId, int receiverId, string content)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return new JsonResult(new { error = "You must be logged in to send a message." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            if (string.IsNullOrEmpty(content) || senderId <= 0 || receiverId <= 0)
            {
                return BadRequest("Invalid message or user IDs.");
            }

            var message = new Message
            {
                OrderID = orderId,
                SenderID = senderId,
                ReceiverID = receiverId,
                Content = content,
                SentDate = DateTime.Now,
                IsRead = false
            };

            _context.Message.Add(message);
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                message.MessageID,
                message.Content,
                message.SentDate
            });
        }
    }
}