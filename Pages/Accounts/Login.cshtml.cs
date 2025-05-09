using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Shofy.Data;
using Shofy.Models;
using Microsoft.EntityFrameworkCore;
using Shofy.Helpers;
using Microsoft.Extensions.Logging;
using System;

namespace Shofy.Pages.Accounts
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly ShofyContext _context;

        public LoginModel(ILogger<LoginModel> logger, ShofyContext context)
        {
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập username.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username phải từ 3 đến 50 ký tự.")]
        public string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(256, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 256 ký tự.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("OnPostAsync called with Username: {Username}", Username);

            Username = Username?.Trim();
            Password = Password?.Trim();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return Page();
            }

            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Username.ToLower() == Username.ToLower());

                // Kiểm tra tài khoản tồn tại, mật khẩu đúng và đang hoạt động
                if (user == null || !BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash) || !user.IsActive)
                {
                    _logger.LogWarning("Login failed for Username: {Username}. Reason: {Reason}", Username,
                        user == null ? "User not found" :
                        (!user.IsActive ? "User is inactive" : "Password incorrect"));

                    ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng hoặc tài khoản đã bị vô hiệu hóa.";
                    return Page();
                }

                _logger.LogInformation("Login successful for Username: {Username}", Username);
                HttpContext.Session.SetUserId(user.UserID);
                HttpContext.Session.SetUsername(user.Username);
                HttpContext.Session.SetUserRole(user.Role);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("Avatar", user.Avatar ?? "/images/noavt.jpg");

                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn cơ sở dữ liệu trong quá trình đăng nhập.");
                ErrorMessage = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.";
                return Page();
            }
        }
    }
}
