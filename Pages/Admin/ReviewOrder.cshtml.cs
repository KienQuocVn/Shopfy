using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;

namespace Shofy.Pages.Admin
{
    public class ReviewOrderModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<ReviewOrderModel> _logger;

        public ReviewOrderModel(ShofyContext context, ILogger<ReviewOrderModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Order Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Lấy thông tin đơn hàng từ cơ sở dữ liệu, bao gồm User và OrderDetails
            Order = await _context.Order
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (Order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToPage("/Admin/OrderIndex");
            }

            return Page();
        }
    }
}