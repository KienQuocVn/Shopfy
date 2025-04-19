using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Shofy.Data;
using Shofy.Models;
using Shofy.Helpers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using System.IO;
using System;

namespace Shofy.Pages.Accounts
{
    public class ProfileModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<ProfileModel> _logger;
        private readonly IWebHostEnvironment _environment;

        public ProfileModel(ShofyContext context, ILogger<ProfileModel> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        public User? User { get; set; }

        [BindProperty]
        public UserUpdateModel UpdateModel { get; set; } = new UserUpdateModel();

        [BindProperty]
        public AvatarUpdateModel AvatarModel { get; set; } = new AvatarUpdateModel();

        [BindProperty]
        public PasswordUpdateModel PasswordModel { get; set; } = new PasswordUpdateModel();

        public class UserUpdateModel
        {
            public int UserID { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
            [EmailAddress(ErrorMessage = "Vui lòng nhập email hợp lệ.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
            [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
            public string FullName { get; set; }

            [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
            [RegularExpression(@"^(?:\+84|0)(?:3[2-9]|5[6-9]|7[0-9]|8[1-9]|9[0-9])[0-9]{7}$",
                ErrorMessage = "Vui lòng nhập số điện thoại hợp lệ (ví dụ: 0919925302 hoặc +84919925302).")]
            public string? PhoneNumber { get; set; }

            [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự.")]
            public string? Address { get; set; }
        }

        public class AvatarUpdateModel
        {
            public IFormFile? AvatarFile { get; set; }
        }

        public class PasswordUpdateModel
        {
            [DataType(DataType.Password)]
            public string CurrentPassword { get; set; }

            [StringLength(256, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải từ 8 đến 256 ký tự.")]
            [DataType(DataType.Password)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
                ErrorMessage = "Mật khẩu mới phải chứa ít nhất một chữ cái in hoa, một chữ cái thường, một số và một ký tự đặc biệt.")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmNewPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            int? userId = HttpContext.Session.GetUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem hồ sơ.";
                return RedirectToPage("/Accounts/Login");
            }

            User = await _context.User.AsNoTracking().FirstOrDefaultAsync(u => u.UserID == userId);
            if (User == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToPage("/Accounts/Login");
            }

            UpdateModel = new UserUpdateModel
            {
                UserID = User.UserID,
                Email = User.Email,
                FullName = User.FullName ?? "",
                PhoneNumber = User.PhoneNumber,
                Address = User.Address
            };

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            int? userId = HttpContext.Session.GetUserId();
            if (userId == null) return RedirectToLoginPage();

            var existingUser = await _context.User.FindAsync(userId);
            if (existingUser == null) return RedirectToLoginPage("Không tìm thấy người dùng.");

            // **Xóa hoàn toàn ModelState**
            ModelState.Clear();

            // Chỉ validate UpdateModel
            if (TryValidateModel(UpdateModel, nameof(UpdateModel)))
            {
                UpdateModel.Email = UpdateModel.Email?.Trim();
                UpdateModel.FullName = UpdateModel.FullName?.Trim();
                UpdateModel.PhoneNumber = UpdateModel.PhoneNumber?.Trim();
                UpdateModel.Address = UpdateModel.Address?.Trim();

                if (UpdateModel.Email != existingUser.Email &&
                    await _context.User.AnyAsync(u => u.Email.ToLower() == UpdateModel.Email.ToLower() && u.UserID != userId))
                {
                    ModelState.AddModelError("UpdateModel.Email", "Email đã được sử dụng.");
                }

                if (ModelState.IsValid)
                {
                    existingUser.Email = UpdateModel.Email;
                    existingUser.FullName = UpdateModel.FullName;
                    existingUser.PhoneNumber = UpdateModel.PhoneNumber;
                    existingUser.Address = UpdateModel.Address;

                    try
                    {
                        _context.Entry(existingUser).State = EntityState.Modified;
                        int changes = await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = changes > 0 ? "Cập nhật hồ sơ thành công!" : "Không có thay đổi nào được thực hiện.";
                        return RedirectToPage(); // Redirect để làm mới ModelState
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ cho UserID: {UserId}", userId);
                        TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật hồ sơ. Vui lòng thử lại.";
                    }
                }
            }
            else
            {
                _logger.LogWarning("ModelState invalid for UpdateModel: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            User = existingUser;
            return Page();
        }

        private IActionResult RedirectToLoginPage(string? errorMessage = null)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                TempData["ErrorMessage"] = errorMessage;
            }
            return RedirectToPage("/Accounts/Login");
        }

        public async Task<IActionResult> OnPostUpdateAvatarAsync()
        {
            int? userId = HttpContext.Session.GetUserId();
            if (userId == null) return RedirectToLoginPage();

            var existingUser = await _context.User.FindAsync(userId);
            if (existingUser == null) return RedirectToLoginPage("Không tìm thấy người dùng.");

            // **Xóa hoàn toàn ModelState**
            ModelState.Clear();

            // Chỉ validate AvatarModel
            if (TryValidateModel(AvatarModel, nameof(AvatarModel)))
            {
                if (AvatarModel.AvatarFile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(AvatarModel.AvatarFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("AvatarModel.AvatarFile", "Chỉ hỗ trợ các định dạng hình ảnh JPG, JPEG, PNG.");
                    }

                    if (AvatarModel.AvatarFile.Length > 2 * 1024 * 1024) // 2MB limit
                    {
                        ModelState.AddModelError("AvatarModel.AvatarFile", "Hình ảnh không được vượt quá 2MB.");
                    }

                    if (ModelState.IsValid)
                    {
                        var avatarsPath = Path.Combine(_environment.WebRootPath, "images", "avatars");
                        if (!Directory.Exists(avatarsPath)) Directory.CreateDirectory(avatarsPath);

                        var fileName = $"user{userId}_{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(avatarsPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await AvatarModel.AvatarFile.CopyToAsync(stream);
                        }

                        existingUser.Avatar = $"/images/avatars/{fileName}";

                        try
                        {
                            _context.Entry(existingUser).State = EntityState.Modified;
                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = "Cập nhật ảnh đại diện thành công!";
                            return RedirectToPage(); // Redirect để làm mới ModelState
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi cập nhật ảnh đại diện cho UserID: {UserId}", userId);
                            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật ảnh đại diện. Vui lòng thử lại.";
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("AvatarModel.AvatarFile", "Vui lòng chọn một hình ảnh.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState invalid for AvatarModel: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            User = existingUser;
            return Page();
        }

        public async Task<IActionResult> OnPostUpdatePasswordAsync()
        {
            int? userId = HttpContext.Session.GetUserId();
            if (userId == null) return RedirectToLoginPage();

            var existingUser = await _context.User.FindAsync(userId);
            if (existingUser == null) return RedirectToLoginPage("Không tìm thấy người dùng.");

            // **Xóa hoàn toàn ModelState**
            ModelState.Clear();

            // Chỉ validate PasswordModel
            if (TryValidateModel(PasswordModel, nameof(PasswordModel)))
            {
                if (!BCrypt.Net.BCrypt.Verify(PasswordModel.CurrentPassword, existingUser.PasswordHash))
                {
                    ModelState.AddModelError("PasswordModel.CurrentPassword", "Mật khẩu hiện tại không đúng.");
                }

                if (ModelState.IsValid)
                {
                    existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordModel.NewPassword);

                    try
                    {
                        _context.Entry(existingUser).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                        return RedirectToPage(); // Redirect để làm mới ModelState
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi đổi mật khẩu cho UserID: {UserId}", userId);
                        TempData["ErrorMessage"] = "Đã xảy ra lỗi khi đổi mật khẩu. Vui lòng thử lại.";
                    }
                }
            }
            else
            {
                _logger.LogWarning("ModelState invalid for PasswordModel: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            User = existingUser;
            return Page();
        }
    }
}