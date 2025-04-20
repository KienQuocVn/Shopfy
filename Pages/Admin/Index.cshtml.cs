using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Shofy.Pages.Admin
{
    //[Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ShofyContext _context;

        public DashboardModel(ShofyContext context)
        {
            _context = context;
        }

        // Properties for the charts data
        public List<int> OrderCounts { get; set; }
        public List<int> UserCounts { get; set; }

        // Data for the page
        public string Search { get; set; }

        // Get method to fetch and set data for charts
        public async Task<IActionResult> OnGetAsync()
        {
            // Fetch the number of orders per month
            OrderCounts = await GetOrderCountsAsync();

            // Fetch the number of users per month
            UserCounts = await GetUserCountsAsync();

            return Page();
        }

        // Fetching order counts for each month from the database
        private async Task<List<int>> GetOrderCountsAsync()
        {
            var currentYear = DateTime.Now.Year;

            // Grouping orders by month of the current year
            var orderCounts = await _context.Order
                .Where(o => o.OrderedDate.Year == currentYear)
                .GroupBy(o => o.OrderedDate.Month)
                .OrderBy(g => g.Key)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            // Initialize the list with 0 for all 12 months
            var monthlyCounts = Enumerable.Range(1, 12)
                                   .Select(month => orderCounts.FirstOrDefault(x => x.Month == month)?.Count ?? 0)
                                   .ToList();

            return monthlyCounts;
        }

        // Fetching user counts for each month from the database
        private async Task<List<int>> GetUserCountsAsync()
        {
            var currentYear = DateTime.Now.Year;

            // Grouping users by month of account creation for the current year
            var userCounts = await _context.User
                .Where(u => u.CreatedDate.Year == currentYear)
                .GroupBy(u => u.CreatedDate.Month)
                .OrderBy(g => g.Key)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            // Initialize the list with 0 for all 12 months
            var monthlyUserCounts = Enumerable.Range(1, 12)
                                      .Select(month => userCounts.FirstOrDefault(x => x.Month == month)?.Count ?? 0)
                                      .ToList();

            return monthlyUserCounts;
        }
    }
}
