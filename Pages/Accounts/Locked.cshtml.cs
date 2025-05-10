using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shofy.Pages.Accounts
{
    public class LockedModel : PageModel
    {
        public string Username { get; set; }
        public string Reason { get; set; }

        public IActionResult OnGet()
        {
            // Kiểm tra nếu không có TempData thì chuyển về trang Login
            if (!TempData.ContainsKey("LockedUsername") || !TempData.ContainsKey("LockedReason"))
            {
                return RedirectToPage("/Accounts/Login");
            }

            Username = TempData["LockedUsername"]?.ToString();
            Reason = TempData["LockedReason"]?.ToString();

            return Page();
        }
    }
}
