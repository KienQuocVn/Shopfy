using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;

namespace Shofy.Pages.Admin
{
    public class ReviewProductModel : PageModel
    {
        private readonly ShofyContext _context;

        public ReviewProductModel(ShofyContext context)
        {
            _context = context;
        }

        public Product Product { get; set; }

        public string Username { get; set; }
        public string Role { get; set; }
        public string Avatar { get; set; }

        public IActionResult OnGet(int id)
        {
            // Retrieve the product based on the product ID
            Product = _context.Product.FirstOrDefault(p => p.ProductID == id);
            if (Product == null)
            {
                return NotFound();
            }

            // Get session data
            Username = HttpContext.Session.GetString("Username") ?? "Guest";
            Role = HttpContext.Session.GetString("Role") ?? "Unknown";
            Avatar = HttpContext.Session.GetString("Avatar") ?? "/images/noavt.jpg";

            return Page();
        }
    }
}
