using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;
using Shofy.Helpers;
using System;
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

        [BindProperty(SupportsGet = true)]
        public string DateRange { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Please log in to view your order history.";
                return RedirectToPage("/Accounts/Login");
            }

            var query = _context.Order
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.UserID == userId.Value);

            // Apply Date Range Filter
            if (!string.IsNullOrEmpty(DateRange))
            {
                DateTime startDate = DateTime.MinValue;
                switch (DateRange)
                {
                    case "Last30Days":
                        startDate = DateTime.Now.AddDays(-30);
                        break;
                    case "Last3Months":
                        startDate = DateTime.Now.AddMonths(-3);
                        break;
                    case "Last6Months":
                        startDate = DateTime.Now.AddMonths(-6);
                        break;
                    case "LastYear":
                        startDate = DateTime.Now.AddYears(-1);
                        break;
                }
                if (startDate != DateTime.MinValue)
                {
                    query = query.Where(o => o.OrderedDate >= startDate);
                }
            }

            // Apply Status Filter
            if (!string.IsNullOrEmpty(Status) && Status != "All Status")
            {
                query = query.Where(o => o.Status == Status);
            }

            // Apply Search Filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                // Search by order number (e.g., "#ORD-123")
                if (SearchTerm.StartsWith("#ORD-", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(SearchTerm.Replace("#ORD-", ""), out int orderId))
                    {
                        query = query.Where(o => o.OrderID == orderId);
                    }
                }
                else
                {
                    // Search by product name in OrderDetails
                    query = query.Where(o => o.OrderDetails.Any(od => od.Product.Name.Contains(SearchTerm)));
                }
            }

            Orders = await query
                .OrderByDescending(o => o.OrderedDate)
                .ToListAsync();

            return Page();
        }
    }
}