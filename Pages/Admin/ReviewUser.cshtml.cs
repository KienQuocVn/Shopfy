using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;

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

        public IActionResult OnGet(int UserId)
        {
            User = _context.User.Find(UserId);
            if (User == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
