using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
using Microsoft.AspNetCore.Http; // For HttpContext

namespace Shofy.Pages.Admin
{
    public class ReviewUserModel : PageModel
    {
        private readonly ShofyContext _context;

        public ReviewUserModel(ShofyContext context)
        {
            _context = context;
        }

        public User User { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string Avatar { get; set; }

        public IActionResult OnGet(int UserId)
        {
            // Retrieve session data
            Username = HttpContext.Session.GetString("Username") ?? "Guest";
            Role = HttpContext.Session.GetString("Role") ?? "Unknown";
            Avatar = HttpContext.Session.GetString("Avatar") ?? "/images/noavt.jpg";
            
            // Retrieve user data
            User = _context.User.Find(UserId);
            if (User == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
