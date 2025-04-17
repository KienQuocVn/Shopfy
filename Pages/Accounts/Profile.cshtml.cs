using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Shofy.Data;
using Shofy.Models;
using Shofy.Helpers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Shofy.Pages.Accounts
{
    public class ProfileModel : PageModel
    {
        private readonly ShofyContext _context;

        public ProfileModel(ShofyContext context)
        {
            _context = context;
        }

        public User? User { get; set; } // Remove [BindProperty] to avoid validation

        [BindProperty]
        public UserUpdateModel UpdateModel { get; set; } = new UserUpdateModel();

        public class UserUpdateModel
        {
            public int UserID { get; set; }

            [StringLength(100), EmailAddress]
            public string? Email { get; set; }

            public string? FullName { get; set; }

            public string? PhoneNumber { get; set; }

            public string? Address { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            int? userId = HttpContext.Session.GetUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please log in to view your profile.";
                return RedirectToPage("/Accounts/Login");
            }

            User = await _context.User.AsNoTracking().FirstOrDefaultAsync(u => u.UserID == userId);
            if (User == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage("/Accounts/Login");
            }

            // Initialize UpdateModel for the form
            UpdateModel = new UserUpdateModel
            {
                UserID = User.UserID,
                Email = User.Email,
                FullName = User.FullName,
                PhoneNumber = User.PhoneNumber,
                Address = User.Address
            };

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            int? userId = HttpContext.Session.GetUserId();
            Console.WriteLine($"Session UserID: {userId}");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToPage("/Accounts/Login");
            }

            var existingUser = await _context.User.FindAsync(userId);
            if (existingUser == null)
            {
                Console.WriteLine("User not found in database.");
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage("/Accounts/Login");
            }

            // Log form data
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"Form Key: {key}, Value: {Request.Form[key]}");
            }

            // Clear ModelState for the entire User model to avoid validation
            ModelState.ClearValidationState("User");
            ModelState.Remove("User");

            // Validate only UpdateModel
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                    }
                }
                User = existingUser;
                return Page();
            }

            // Update fields only if new values are provided
            if (!string.IsNullOrEmpty(UpdateModel.Email))
            {
                existingUser.Email = UpdateModel.Email;
            }

            if (UpdateModel.FullName != null)
            {
                existingUser.FullName = UpdateModel.FullName;
            }

            if (UpdateModel.PhoneNumber != null)
            {
                existingUser.PhoneNumber = UpdateModel.PhoneNumber;
            }

            if (UpdateModel.Address != null)
            {
                existingUser.Address = UpdateModel.Address;
            }

            // Log updated values
            Console.WriteLine($"Updated Email: {existingUser.Email}");
            Console.WriteLine($"Updated FullName: {existingUser.FullName}");
            Console.WriteLine($"Updated PhoneNumber: {existingUser.PhoneNumber}");
            Console.WriteLine($"Updated Address: {existingUser.Address}");

            // Save changes
            try
            {
                _context.Entry(existingUser).State = EntityState.Modified;
                int changes = await _context.SaveChangesAsync();
                Console.WriteLine($"Number of changes saved: {changes}");
                TempData["SuccessMessage"] = changes > 0 ? "Profile updated successfully!" : "No changes were made.";
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"DbUpdateException: {dbEx.Message}, Inner: {dbEx.InnerException?.Message}");
                TempData["ErrorMessage"] = "An error occurred while updating your profile. Please try again.";
                User = existingUser;
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}, StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An error occurred while updating your profile. Please try again.";
                User = existingUser;
                return Page();
            }

            User = existingUser;
            UpdateModel = new UserUpdateModel
            {
                UserID = User.UserID,
                Email = User.Email,
                FullName = User.FullName,
                PhoneNumber = User.PhoneNumber,
                Address = User.Address
            };

            return Page();
        }
    }
}