using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
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

        // Properties for the charts data
        public List<int> OrderCounts { get; set; }
        public List<int> UserCounts { get; set; }

        // Data for the page
        public string Search { get; set; }

        // Get method to fetch and set data for charts
        public async Task<IActionResult> OnGetAsync()
        {
            // Simulate or fetch the number of orders per month
            OrderCounts = await GetOrderCountsAsync();

            // Simulate or fetch the number of users per month
            UserCounts = await GetUserCountsAsync();

            return Page();
        }

        // Function to simulate fetching order counts for each month
        private Task<List<int>> GetOrderCountsAsync()
        {
            // Example data representing order counts for the months
            // Replace this with actual data fetch from database if necessary
            return Task.FromResult(new List<int> { 12, 19, 3, 5, 2 });
        }

        // Function to simulate fetching user counts for each month
        private Task<List<int>> GetUserCountsAsync()
        {
            // Example data representing user counts for the months
            // Replace this with actual data fetch from database if necessary
            return Task.FromResult(new List<int> { 50, 75, 120, 150, 200 });
        }
    }
}
