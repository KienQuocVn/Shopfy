using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
using Shofy.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Shofy.Pages.Accounts

{
    public class RegisterModel : PageModel
    {
        private readonly ShofyContext _db;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(ShofyContext db, ILogger<RegisterModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập username.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username phải từ 3 đến 50 ký tự.")]
        public required string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        public required string FullName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Vui lòng nhập email hợp lệ.")]
        [StringLength(100)]
        public required string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(256, MinimumLength = 6, ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("OnPostAsync called");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return Page();
            }

            var usernameExists = await _db.User.AnyAsync(u => u.Username == Username);
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Username đã tồn tại.");
            }

            var emailExists = await _db.User.AnyAsync(u => u.Email == Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new User
            {
                Username = Username,
                FullName = FullName,
                Email = Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
                Role = "user",
                CreatedDate = DateTime.Now
            };

            try
            {
                _db.User.Add(user);
                await _db.SaveChangesAsync();
                _logger.LogInformation("User {Username} registered successfully", Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu vào DB");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu dữ liệu. Vui lòng thử lại.");
                return Page();
            }

            HttpContext.Session.SetUserId(user.UserID);
            HttpContext.Session.SetUsername(user.Username);
            HttpContext.Session.SetUserRole(user.Role);

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToPage("/Accounts/Login");
        }
    }
}
