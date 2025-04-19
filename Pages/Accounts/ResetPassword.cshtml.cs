using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Shofy.Data;
using Shofy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace Shofy.Pages.Accounts
{
    public class ResetPasswordModel : PageModel
    {
        private readonly ILogger<ResetPasswordModel> _logger;
        private readonly ShofyContext _context;

        public ResetPasswordModel(ILogger<ResetPasswordModel> logger, ShofyContext context)
        {
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập mã xác nhận.")]
        public string ResetCode { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(256, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 256 ký tự.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet(string email)
        {
            Email = email;
            if (string.IsNullOrEmpty(Email))
            {
                ErrorMessage = "Email không hợp lệ. Vui lòng yêu cầu mã xác nhận lại.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("OnPostResetPasswordAsync called with Email: {Email}", Email);

            Email = Email?.Trim();
            ResetCode = ResetCode?.Trim();
            NewPassword = NewPassword?.Trim();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return Page();
            }

            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Email.ToLower() == Email.ToLower());
                if (user == null)
                {
                    _logger.LogWarning("User not found for Email: {Email}", Email);
                    ErrorMessage = "Email không tồn tại.";
                    return Page();
                }

                if (user.ResetPasswordCode != ResetCode || user.ResetPasswordCodeExpiration < DateTime.UtcNow)
                {
                    _logger.LogWarning("Invalid or expired reset code for Email: {Email}", Email);
                    ErrorMessage = "Mã xác nhận không hợp lệ hoặc đã hết hạn.";
                    return Page();
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                user.ResetPasswordCode = null;
                user.ResetPasswordCodeExpiration = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Password reset successful for Email: {Email}", Email);
                TempData["SuccessMessage"] = "Mật khẩu đã được đặt lại thành công.";
                return RedirectToPage("/Accounts/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đặt lại mật khẩu cho email: {Email}", Email);
                ErrorMessage = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.";
                return Page();
            }
        }
    }
}