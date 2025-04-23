using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace Shofy.Pages.Admin
{
    public class EditUserModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<EditUserModel> _logger;

        public EditUserModel(ShofyContext context, ILogger<EditUserModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public int UserId { get; set; }

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
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Vui lòng nhập số điện thoại hợp lệ.")]
        public string PhoneNumber { get; set; }

        [BindProperty]
        public string Address { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn quyền.")]
        public string Role { get; set; }

        public async Task<IActionResult> OnGetAsync(int userId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToPage("/Error");
            }

            var user = await _context.User.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Không tìm thấy người dùng với ID: {UserId}", userId);
                return NotFound();
            }

            UserId = user.UserID;
            Username = user.Username;
            FullName = user.FullName;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
            Address = user.Address;
            Role = user.Role;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToPage("/Error");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ.");
                return Page();
            }

            var userInDb = await _context.User.FindAsync(UserId);
            if (userInDb == null)
            {
                _logger.LogWarning("Không tìm thấy người dùng.");
                return NotFound();
            }

            userInDb.Username = Username;
            userInDb.FullName = FullName;
            userInDb.Email = Email;
            userInDb.PhoneNumber = PhoneNumber;
            userInDb.Address = Address;
            userInDb.Role = Role;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cập nhật người dùng thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật người dùng.");
                ModelState.AddModelError("", "Đã xảy ra lỗi. Vui lòng thử lại sau.");
                return Page();
            }

            TempData["SuccessMessage"] = "Cập nhật thông tin người dùng thành công!";
            return RedirectToPage("User");
        }
    }
}
