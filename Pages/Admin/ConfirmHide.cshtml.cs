using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
using System.Linq;

namespace Shofy.Pages.Admin
{
    public class ConfirmHideModel : PageModel
    {
        private readonly ShofyContext _context;

        public ConfirmHideModel(ShofyContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int UserId { get; set; }

        [BindProperty]
        public string Reason { get; set; } = "";

        public IActionResult OnGet(int userId)
        {
            var user = _context.User.Find(userId);
            if (user == null) return NotFound();

            UserId = userId;
            return Page();
        }

        public IActionResult OnPost()
        {
            var user = _context.User.Find(UserId);
            if (user == null) return NotFound();

            // Kiểm tra nếu người dùng có đơn hàng
            bool hasOrders = _context.Order.Any(o => o.UserID == UserId);
            if (hasOrders)
            {
                TempData["Error"] = "Người dùng có đơn hàng nên không thể ẩn.";
                return RedirectToPage("User");
            }

            user.IsActive = false;
            user.HiddenReason = Reason;
            _context.Update(user);
            _context.SaveChanges();

            return RedirectToPage("User");
        }
    }
}
