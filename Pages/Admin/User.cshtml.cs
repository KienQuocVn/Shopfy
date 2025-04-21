using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Shofy.Data;
using Shofy.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public string RoleFilter { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin")
            {
                return RedirectToPage("/Error");
            }

            // Đảm bảo pageNumber không nhỏ hơn 1
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            CurrentPage = pageNumber;

            var query = _context.User.AsQueryable();

            if (!string.IsNullOrEmpty(RoleFilter))
            {
                query = query.Where(u => u.Role == RoleFilter);
            }

            var totalUsers = query.Count();
            TotalPages = (int)System.Math.Ceiling(totalUsers / (double)PageSize);

            Users = query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }


        public IActionResult OnPostDelete(int UserId, int CurrentPage, string RoleFilter)
        {
            var user = _context.User.Find(UserId);
            if (user != null)
            {
                _context.User.Remove(user);
                _context.SaveChanges();
                _logger.LogInformation("Xóa người dùng với mã {UserId}", UserId);
            }
            else
            {
                TempData["Error"] = "User not found.";
                _logger.LogWarning("Không tìm thấy người dùng với mã ID {UserId}", UserId);
            }

            return RedirectToPage(new { pageNumber = CurrentPage, RoleFilter = RoleFilter });
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
