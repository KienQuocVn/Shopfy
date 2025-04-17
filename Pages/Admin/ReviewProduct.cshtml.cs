using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;

namespace Shofy.Pages.Admin
{
    public class ReviewProductModel : PageModel
    {
        private readonly ShofyContext _context;

        public ReviewProductModel(ShofyContext context)
        {
            _context = context;
        }

        public Product Product { get; set; }

        public IActionResult OnGet(int id)
        {
            Product = _context.Product.FirstOrDefault(p => p.ProductID == id);
            if (Product == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
