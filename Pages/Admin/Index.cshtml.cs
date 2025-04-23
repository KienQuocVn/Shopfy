using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
namespace Shofy.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        
        private readonly ShofyContext _context;

        public DashboardModel(ShofyContext context)
        {
            _context = context;
        }

        public List<int> OrderCounts { get; set; }
        public List<int> UserCounts { get; set; }
        public List<decimal> RevenuePerMonth { get; set; }
        public List<int> ProductQuantityPerMonth { get; set; }
        

        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra quyền của người dùng từ session
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                // Nếu không phải Admin, chuyển hướng đến trang lỗi
                return RedirectToPage("/Error"); 
            }

            // Gọi các phương thức lấy dữ liệu
            OrderCounts = await GetOrderCountsAsync();
            UserCounts = await GetUserCountsAsync();
            RevenuePerMonth = await GetRevenuePerMonthAsync();
            ProductQuantityPerMonth = await GetProductQuantityPerMonthAsync();

            return Page();
        }

        private async Task<List<int>> GetOrderCountsAsync()
        {
            var currentYear = DateTime.Now.Year;

            var orderCounts = await _context.Order
                .Where(o => o.OrderedDate.Year == currentYear)
                .GroupBy(o => o.OrderedDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            return Enumerable.Range(1, 12)
                .Select(m => orderCounts.FirstOrDefault(x => x.Month == m)?.Count ?? 0)
                .ToList();
        }

        private async Task<List<int>> GetUserCountsAsync()
        {
            var currentYear = DateTime.Now.Year;

            var userCounts = await _context.User
                .Where(u => u.CreatedDate.Year == currentYear)
                .GroupBy(u => u.CreatedDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            return Enumerable.Range(1, 12)
                .Select(m => userCounts.FirstOrDefault(x => x.Month == m)?.Count ?? 0)
                .ToList();
        }

        private async Task<List<decimal>> GetRevenuePerMonthAsync()
        {
            var currentYear = DateTime.Now.Year;

            var revenue = await _context.Order
                .Where(o => o.OrderedDate.Year == currentYear)
                .GroupBy(o => o.OrderedDate.Month)
                .Select(g => new { Month = g.Key, TotalRevenue = g.Sum(o => o.TotalPrice) })
                .ToListAsync();

            return Enumerable.Range(1, 12)
                .Select(m => revenue.FirstOrDefault(x => x.Month == m)?.TotalRevenue ?? 0)
                .ToList();
        }

        private async Task<List<int>> GetProductQuantityPerMonthAsync()
        {
            var currentYear = DateTime.Now.Year;

            var productQuantities = await _context.OrderDetail
                .Where(od => od.Order.OrderedDate.Year == currentYear)
                .GroupBy(od => od.Order.OrderedDate.Month)
                .Select(g => new { Month = g.Key, TotalQuantity = g.Sum(od => od.Quantity) })
                .ToListAsync();

            return Enumerable.Range(1, 12)
                .Select(m => productQuantities.FirstOrDefault(x => x.Month == m)?.TotalQuantity ?? 0)
                .ToList();
        }
    }
}
