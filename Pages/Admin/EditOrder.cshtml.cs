using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;

namespace Shofy.Pages.Admin
{
    public class EditOrderModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<EditOrderModel> _logger;

        public EditOrderModel(ShofyContext context, ILogger<EditOrderModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Thông tin đơn hàng cần chỉnh sửa
        [BindProperty]
        public Order Order { get; set; }

        // Thông tin người dùng (Username, Role, Avatar) từ session
        public string Username { get; set; }
        public string Role { get; set; }
        public string Avatar { get; set; }

        // Xử lý GET: Lấy thông tin đơn hàng từ database
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _context.Order
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (Order == null)
            {
                return NotFound();
            }

            // Get session variables
            Username = HttpContext.Session.GetString("Username") ?? "Guest";
            Role = HttpContext.Session.GetString("Role") ?? "Unknown";
            Avatar = HttpContext.Session.GetString("Avatar") ?? "/images/noavt.jpg";

            return Page();
        }

        // Xử lý POST: Cập nhật đơn hàng
        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var orderToUpdate = await _context.Order.FindAsync(id);

            if (orderToUpdate == null)
            {
                return NotFound();
            }

            // Cập nhật các trường cần thiết
            orderToUpdate.TotalPrice = Order.TotalPrice;
            orderToUpdate.PaymentMethod = Order.PaymentMethod;
            orderToUpdate.Status = Order.Status;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Order with ID {id} updated successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("/Admin/Order"); // Quay lại trang danh sách đơn hàng
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderID == id);
        }
    }
}
