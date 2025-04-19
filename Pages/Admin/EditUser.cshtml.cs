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
        public User NewUser { get; set; }


        public IActionResult OnGet(int UserId)
        {
            NewUser = _context.User.Find(UserId);
            if (User == null)
            {
                return NotFound();
            }
            return Page();
        }

        public IActionResult OnPost()
        {

            if (NewUser == null)
            {
                return BadRequest("NewUser cannot be null.");
            }
            var userInDb = _context.User.Find(NewUser.UserID);
            if (userInDb == null)
            {
                return NotFound();
            }

            userInDb.Username = NewUser.Username;
            userInDb.Email = NewUser.Email;
            userInDb.Role = NewUser.Role;
            userInDb.FullName = NewUser.FullName;
            userInDb.PhoneNumber = NewUser.PhoneNumber;
            userInDb.Address = NewUser.Address;

            _context.SaveChanges();

            return RedirectToPage("User");
        }
    }
}