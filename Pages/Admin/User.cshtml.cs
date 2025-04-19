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
        public const int PageSize = 10;

        [BindProperty(SupportsGet = true)]
        public string RoleFilter { get; set; } // Role cần lọc

        public void OnGet(int pageNumber = 1)
        {
            CurrentPage = pageNumber;

            var query = _context.User.AsQueryable();

            // Lọc theo Role nếu có
            if (!string.IsNullOrEmpty(RoleFilter))
            {
                query = query.Where(u => u.Role == RoleFilter);
            }

            var totalUsers = query.Count();
            TotalPages = (int)Math.Ceiling(totalUsers / (double)PageSize);

            Users = query
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
            return RedirectToPage(new { RoleFilter, pageNumber = CurrentPage });
        }

        public IActionResult OnGetEdit(int UserId)
        {
            var user = _context.User.Find(UserId);
            if (user == null) return NotFound();
            return RedirectToPage("EditUser", new { UserId });
        }

        public IActionResult OnGetReview(int UserId)
        {
            var user = _context.User.Find(UserId);
            if (user == null) return NotFound();
            return RedirectToPage("ReviewUser", new { UserId });
        }
    }
}
