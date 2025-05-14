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

        [BindProperty(SupportsGet = true)]
        public bool? IsActiveFilter { get; set; }


        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin")
            {
                return RedirectToPage("/Error");
            }

            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            CurrentPage = pageNumber;

            var query = _context.User.AsQueryable();

            if (IsActiveFilter.HasValue)
            {
                query = query.Where(u => u.IsActive == IsActiveFilter.Value);
            }
            else
            {
                query = query.Where(u => u.IsActive);
            }

            if (!string.IsNullOrEmpty(RoleFilter))
            {
                query = query.Where(u => u.Role == RoleFilter);
            }

            var totalUsers = query.Count();
            TotalPages = (int)System.Math.Ceiling(totalUsers / (double)PageSize);

            Users = query
                .OrderBy(u => u.UserID)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }

        public IActionResult OnPostDelete(int UserId, int CurrentPage, string RoleFilter, bool? IsActiveFilter)
        {
            var user = _context.User.Find(UserId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                _logger.LogWarning("Không tìm thấy người dùng với ID {UserId}", UserId);
                return RedirectToPage(new { pageNumber = CurrentPage, RoleFilter, IsActiveFilter });
            }

            bool hasOrders = _context.Order.Any(o => o.UserID == UserId);
            if (hasOrders)
            {
                TempData["Error"] = $"Không thể ẩn người dùng với ID {UserId} vì người này đang có đơn hàng.";
                return RedirectToPage(new { pageNumber = CurrentPage, RoleFilter, IsActiveFilter });
            }

            user.IsActive = false;
            user.HiddenReason = "Tài khoản bị vô hiệu hóa bởi Admin.";
            _context.User.Update(user);
            _context.SaveChanges();

            _logger.LogInformation("Ẩn người dùng với ID {UserId}", UserId);
            return RedirectToPage(new { pageNumber = CurrentPage, RoleFilter, IsActiveFilter });
        }

        public IActionResult OnPostShow(int UserId, int CurrentPage, string RoleFilter, bool? IsActiveFilter)
        {
            var user = _context.User.Find(UserId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                _logger.LogWarning("Không tìm thấy người dùng với ID {UserId}", UserId);
                return RedirectToPage(new { pageNumber = CurrentPage, RoleFilter, IsActiveFilter });
            }

            user.IsActive = true;
            user.HiddenReason = null;
            _context.User.Update(user);
            _context.SaveChanges();

            _logger.LogInformation("Hiển thị lại người dùng với ID {UserId}", UserId);
            return RedirectToPage(new { pageNumber = CurrentPage, RoleFilter, IsActiveFilter });
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

        public IActionResult OnGetConfirmHide(int UserId)
        {
            var user = _context.User.Find(UserId);
            if (user == null) return NotFound();
            return RedirectToPage("ConfirmHide", new { userId = UserId });
        }
    }
}
