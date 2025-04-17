using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shofy.Data;
using Shofy.Models;
using Microsoft.EntityFrameworkCore;
using Shofy.Helpers;

namespace Shofy.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ShofyContext _context;

        public IndexModel(ILogger<IndexModel> logger, ShofyContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Property to hold the list of products
        public IList<Product> Products { get; set; } = new List<Product>();

        public async Task OnGetAsync()
        {
            // Load products from the database where Status is Active
            Products = await _context.Product
                .Where(p => p.Status == "Active")
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId)
        {
            var product = await _context.Product.FirstOrDefaultAsync(p => p.ProductID == productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetCart();

            var existingItem = cart.FirstOrDefault(x => x.ProductID == productId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductID = productId,
                    Quantity = 1,
                    Product = product
                });
            }

            HttpContext.Session.SetCart(cart);
            return RedirectToPage();
        }

    }
}