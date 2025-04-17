using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;

namespace Shofy.Pages.Admin
{
    public class EditUserModel : PageModel
    {
        private readonly ShofyContext _context;

        public EditUserModel(ShofyContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public IActionResult OnPost()
        {
            var userInDb = _context.User.Find(User.UserID);
            if (userInDb == null)
            {
                return NotFound();
            }

            userInDb.Username = User.Username;
            userInDb.Email = User.Email;
            userInDb.Role = User.Role;
            userInDb.FullName = User.FullName;
            userInDb.PhoneNumber = User.PhoneNumber;
            userInDb.Address = User.Address;

            _context.SaveChanges();

            return RedirectToPage("User");
        }
    }
}
