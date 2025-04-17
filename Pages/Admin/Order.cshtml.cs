using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;

namespace Shofy.Pages.Admin
{
    public class OrderIndexModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<OrderIndexModel> _logger;

        public OrderIndexModel(ShofyContext context, ILogger<OrderIndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Danh sách đơn hàng
        public List<Order> Orders { get; set; } = new();

        // Chuỗi tìm kiếm
        [BindProperty(SupportsGet = true)]
        public string Search { get; set; } = string.Empty;

        // Xử lý GET và tìm kiếm đơn hàng
        public async Task OnGetAsync()
        {
            var query = _context.Order
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(o =>
                    o.OrderID.ToString().Contains(Search) ||
                    o.Status.Contains(Search) ||
                    o.User.FullName.Contains(Search));
            }

            Orders = await query
                .OrderByDescending(o => o.OrderedDate)
                .ToListAsync();
        }

        // Xử lý xóa đơn hàng
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order != null)
            {
                _context.Order.Remove(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Order with ID {id} deleted.");
            }

            return RedirectToPage();
        }
    }
}
