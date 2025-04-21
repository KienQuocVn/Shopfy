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

        // Number of orders per page
        public const int PageSize = 10;

        public OrderIndexModel(ShofyContext context, ILogger<OrderIndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // List of orders to display
        public List<Order> Orders { get; set; } = new();

        // Current page number
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        // Total number of pages
        public int TotalPages { get; set; }

        // Search keyword
        [BindProperty(SupportsGet = true)]
        public string Search { get; set; } = string.Empty;

        // Status filter
        [BindProperty(SupportsGet = true)]
        public string Status { get; set; }

        // Payment method filter
        [BindProperty(SupportsGet = true)]
        public string PaymentMethod { get; set; }

        // Thêm kiểm tra quyền Role=Admin
        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToPage("/Error");//Chuyển hướng sang trang error nếu ko hợp lệ
            }

            var query = _context.Order
                .Include(o => o.User)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(o =>
                    o.OrderID.ToString().Contains(Search) ||
                    o.Status.Contains(Search) ||
                    o.User.FullName.Contains(Search));
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(Status))
            {
                query = query.Where(o => o.Status == Status);
            }

            // Apply payment method filter
            if (!string.IsNullOrWhiteSpace(PaymentMethod))
            {
                query = query.Where(o => o.PaymentMethod == PaymentMethod);
            }

            // Sort by OrderID in ascending order (from 1 upwards)
            query = query.OrderBy(o => o.OrderID);

            // Get total number of orders matching filters
            var totalOrders = await query.CountAsync();

            // Calculate total pages based on PageSize
            TotalPages = (int)Math.Ceiling(totalOrders / (double)PageSize);

            // Get orders for the current page
            Orders = await query
                .Skip((CurrentPage - 1) * PageSize) // Skip orders from previous pages
                .Take(PageSize) // Take orders for the current page
                .ToListAsync();

            return Page();
        }

        // Xóa đơn hàng
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToPage("/Error"); // Chuyển hướng sang trang error nếu ko hợp lệ
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage();
            }

            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Đã xóa đơn hàng ID: {id}");
            TempData["SuccessMessage"] = $"Đơn hàng ID {id} đã được xóa.";

            return RedirectToPage();
        }

        // Chỉnh sửa đơn hàng
        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToPage("/Error"); // Chuyển hướng sang trang error nếu ko hợp lệ
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage();
            }

            return RedirectToPage("/Admin/EditOrder", new { id });
        }
    }
}
