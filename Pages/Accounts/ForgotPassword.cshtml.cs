using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Shofy.Data;
using Shofy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System;

namespace Shofy.Pages.Accounts
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly ILogger<ForgotPasswordModel> _logger;
        private readonly ShofyContext _context;
        private readonly SmtpClient _smtpClient;
        private readonly MailAddress _fromAddress;

        public ForgotPasswordModel(ILogger<ForgotPasswordModel> logger, ShofyContext context, SmtpClient smtpClient, MailAddress fromAddress)
        {
            _logger = logger;
            _context = context;
            _smtpClient = smtpClient;
            _fromAddress = fromAddress;
        }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostRequestResetAsync()
        {
            _logger.LogInformation("OnPostRequestResetAsync called with Email: {Email}", Email);

            Email = Email?.Trim();
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

                // Generate a 6-digit reset code
                var resetCode = new Random().Next(100000, 999999).ToString();
                user.ResetPasswordCode = resetCode;
                user.ResetPasswordCodeExpiration = DateTime.UtcNow.AddSeconds(30); 
                await _context.SaveChangesAsync();

                // Send email
                var mailMessage = new MailMessage
                {
                    From = _fromAddress,
                    Subject = "Shopfy Password Reset Code",
                    Body = $"Your password reset code is: <b>{resetCode}</b>. This code expires in 30 seconds.",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(user.Email);

                await _smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Reset code sent to Email: {Email}", Email);

                return RedirectToPage("/Accounts/ResetPassword", new { email = Email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi mã xác nhận đến email: {Email}", Email);
                ErrorMessage = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.";
                return Page();
            }
        }
    }
}