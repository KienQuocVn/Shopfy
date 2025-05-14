using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shofy.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly ShofyContext _context;

        public DashboardModel(ShofyContext context)
        {
            _context = context;
        }

        // Stats card properties
        public int TotalOrderCount { get; set; }
        public decimal OrderPercentageChange { get; set; }
        public int TotalUserCount { get; set; }
        public decimal UserPercentageChange { get; set; }
        public decimal TotalMonthlyRevenue { get; set; }
        public decimal RevenuePercentageChange { get; set; }
        public int TotalProductsSold { get; set; }
        public decimal ProductsSoldPercentageChange { get; set; }

        // Chart properties
        public List<int> OrderCounts { get; set; }
        public List<int> UserCounts { get; set; }
        public List<decimal> RevenuePerMonth { get; set; }
        public List<int> ProductQuantityPerMonth { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToPage("/Error");
            }

            // Populate chart data
            OrderCounts = await GetOrderCountsAsync();
            UserCounts = await GetUserCountsAsync();
            RevenuePerMonth = await GetRevenuePerMonthAsync();
            ProductQuantityPerMonth = await GetProductQuantityPerMonthAsync();

            // Populate stats card data
            await PopulateStatsCardDataAsync();

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

        private async Task PopulateStatsCardDataAsync()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var previousMonth = DateTime.Now.AddMonths(-1).Month;
            var previousYear = DateTime.Now.AddMonths(-1).Year;

            // Orders
            TotalOrderCount = await _context.Order
                .Where(o => o.OrderedDate.Month == currentMonth && o.OrderedDate.Year == currentYear)
                .CountAsync();
            var previousOrderCount = await _context.Order
                .Where(o => o.OrderedDate.Month == previousMonth && o.OrderedDate.Year == previousYear)
                .CountAsync();
            OrderPercentageChange = previousOrderCount > 0
                ? (TotalOrderCount - previousOrderCount) * 100m / previousOrderCount
                : 0;

            // Users
            TotalUserCount = await _context.User
                .Where(u => u.CreatedDate.Month == currentMonth && u.CreatedDate.Year == currentYear)
                .CountAsync();
            var previousUserCount = await _context.User
                .Where(u => u.CreatedDate.Month == previousMonth && u.CreatedDate.Year == previousYear)
                .CountAsync();
            UserPercentageChange = previousUserCount > 0
                ? (TotalUserCount - previousUserCount) * 100m / previousUserCount
                : 0;

            // Revenue
            TotalMonthlyRevenue = await _context.Order
                .Where(o => o.OrderedDate.Month == currentMonth && o.OrderedDate.Year == currentYear)
                .SumAsync(o => o.TotalPrice);
            var previousRevenue = await _context.Order
                .Where(o => o.OrderedDate.Month == previousMonth && o.OrderedDate.Year == previousYear)
                .SumAsync(o => o.TotalPrice);
            RevenuePercentageChange = previousRevenue > 0
                ? (TotalMonthlyRevenue - previousRevenue) * 100m / previousRevenue
                : 0;

            // Products Sold
            TotalProductsSold = await _context.OrderDetail
                .Where(od => od.Order.OrderedDate.Month == currentMonth && od.Order.OrderedDate.Year == currentYear)
                .SumAsync(od => od.Quantity);
            var previousProductsSold = await _context.OrderDetail
                .Where(od => od.Order.OrderedDate.Month == previousMonth && od.Order.OrderedDate.Year == previousYear)
                .SumAsync(od => od.Quantity);
            ProductsSoldPercentageChange = previousProductsSold > 0
                ? (TotalProductsSold - previousProductsSold) * 100m / previousProductsSold
                : 0;
        }
    }
}