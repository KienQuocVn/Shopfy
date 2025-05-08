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

        // Lọc trạng thái hoạt động của người dùng (active hoặc không)
        [BindProperty(SupportsGet = true)]
        public bool? IsActiveFilter { get; set; }

        // Profile session
        public string Username { get; set; }
        public string Role { get; set; }
        public string Avatar { get; set; }

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin")
            {
                return RedirectToPage("/Error");
            }

            Username = HttpContext.Session.GetString("Username") ?? "Guest";
            Role = HttpContext.Session.GetString("Role") ?? "Unknown";
            Avatar = HttpContext.Session.GetString("Avatar") ?? "/images/noavt.jpg";

            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            CurrentPage = pageNumber;

            var query = _context.User.AsQueryable(); // Sử dụng AsQueryable để có thể linh hoạt thay đổi điều kiện truy vấn

            // Nếu lọc theo trạng thái hoạt động (active hoặc inactive)
            if (IsActiveFilter.HasValue)
            {
                query = query.Where(u => u.IsActive == IsActiveFilter.Value);
            }
            else
            {
                query = query.Where(u => u.IsActive); // Mặc định chỉ lấy user còn hoạt động
            }

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

        // Ẩn người dùng thay vì xóa
        public IActionResult OnPostDelete(int UserId, int CurrentPage, string RoleFilter, bool? IsActiveFilter)
        {
            var user = _context.User.Find(UserId);
            if (user != null)
            {
                user.IsActive = false; // Đánh dấu là người dùng không còn hoạt động (Ẩn)
                _context.User.Update(user);
                _context.SaveChanges();
                _logger.LogInformation("Ẩn người dùng với ID {UserId}", UserId);
            }
            else
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                _logger.LogWarning("Không tìm thấy người dùng với ID {UserId}", UserId);
            }

            return RedirectToPage(new { pageNumber = CurrentPage, RoleFilter = RoleFilter, IsActiveFilter = IsActiveFilter });
        }

        // Chuyển hướng tới trang sửa người dùng
        public IActionResult OnGetEdit(int UserId)
        {
            var user = _context.User.Find(UserId);
            if (user == null) return NotFound();
            return RedirectToPage("EditUser", new { UserId });
        }

        // Chuyển hướng tới trang review người dùng
        public IActionResult OnGetReview(int UserId)
        {
            var user = _context.User.Find(UserId);
            if (user == null) return NotFound();
            return RedirectToPage("ReviewUser", new { UserId });
        }

        // Hiển thị lại người dùng
        public IActionResult OnPostShow(int UserId, int CurrentPage, string RoleFilter, bool? IsActiveFilter)
        {
            var user = _context.User.Find(UserId);
            if (user != null)
            {
                user.IsActive = true; // Đánh dấu người dùng là hoạt động (Hiển thị)
                _context.User.Update(user);
                _context.SaveChanges();
                _logger.LogInformation("Hiển thị người dùng với ID {UserId}", UserId);
            }
            else
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                _logger.LogWarning("Không tìm thấy người dùng với ID {UserId}", UserId);
            }

            return RedirectToPage(new { pageNumber = CurrentPage, RoleFilter = RoleFilter, IsActiveFilter = IsActiveFilter });
        }
    }
}
