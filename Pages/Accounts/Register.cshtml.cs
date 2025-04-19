using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Logging;

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
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username chỉ được chứa chữ cái, số và dấu gạch dưới.")]
        public string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và dấu cách.")]
        public string FullName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Vui lòng nhập email hợp lệ.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 256 ký tự.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất một chữ cái in hoa, một chữ cái thường, một số và một ký tự đặc biệt.")]
        public string Password { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("OnPostAsync called");

            Username = Username?.Trim();
            FullName = FullName?.Trim();
            Email = Email?.Trim();
            Password = Password?.Trim();
            ConfirmPassword = ConfirmPassword?.Trim();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return Page();
            }

            var usernameExists = await _db.User.AnyAsync(u => u.Username.ToLower() == Username.ToLower());
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Username đã tồn tại.");
            }

            var emailExists = await _db.User.AnyAsync(u => u.Email.ToLower() == Email.ToLower());
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
                Email = Email.ToLower(), 
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

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToPage("/Accounts/Login");
        }
    }
}