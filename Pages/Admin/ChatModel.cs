using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shofy.Pages.Admin
{
    public class ChatModel : PageModel
    {
        public string Username { get; set; }
        public string Role { get; set; }
        public string Avatar { get; set; }

        public void OnGet()
        {
            // You can initialize the properties here
            // For example, get the current user's information from your authentication system
            Username = "Admin"; // Replace with actual user data
            Role = "Administrator"; // Replace with actual role
            Avatar = "/images/avatar.png"; // Replace with actual avatar path
        }
    }
}