using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;
using Shofy.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Shofy.Pages.Client
{
    public class OrderHistoryModel : PageModel
    {
        private readonly ShofyContext _context;

        public OrderHistoryModel(ShofyContext context)
        {
            _context = context;
        }

        public IList<Order> Orders { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Please log in to add items to cart.";
                return RedirectToPage("/Accounts/Login");
            }

            Orders = await _context.Order
                .Include(o => o.User)
                .Where(o => o.UserID == userId.Value)
                .OrderByDescending(o => o.OrderedDate)
                .ToListAsync();

            return Page();
        }
    }
}