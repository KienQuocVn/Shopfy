using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;
using System.Threading.Tasks;

namespace Shofy.Pages.Client
{
    public class OrderTrackingModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<OrderTrackingModel> _logger;

        public OrderTrackingModel(ShofyContext context, ILogger<OrderTrackingModel> logger)
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

            

            return Page();
        }
    }
}