using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Shofy.Data;
using Shofy.Models;
using System.Collections.Generic;
using System.Linq;

namespace Shofy.Pages.Admin
{
    public class UserModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<UserModel> _logger;

        public UserModel(ShofyContext context, ILogger<UserModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<User> Users { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int PageSize = 10; // Number of items per page

        public void OnGet(int pageNumber = 1)
        {
            CurrentPage = pageNumber;

            // Get total number of users
            var totalUsers = _context.User.Count();
            TotalPages = (int)Math.Ceiling(totalUsers / (double)PageSize);

            // Get users for the current page
            Users = _context.User
                            .Skip((CurrentPage - 1) * PageSize)
                            .Take(PageSize)
                            .ToList();
        }

        public IActionResult OnPostDelete(int UserId)
        {
            var user = _context.User.Find(UserId);
            if (user != null)
            {
                _context.User.Remove(user);
                _context.SaveChanges();
                _logger.LogInformation("Deleted user with ID {UserId}", UserId);
            }
            return RedirectToPage();
        }

        public IActionResult OnGetEdit(int UserId)
        {
            var user = _context.User.Find(UserId);
            if (user == null)
            {
                return NotFound();
            }
            return RedirectToPage("EditUser", new { UserId = UserId });
        }

        public IActionResult OnGetReview(int UserId)
        {
            var user = _context.User.Find(UserId);
            if (user == null)
            {
                return NotFound();
            }
            return RedirectToPage("ReviewUser", new { UserId = UserId });
        }
    }
}
