using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
using Shofy.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shofy.Pages.Client
{
    public class Pages_Client_Product_DetailModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<Pages_Client_Product_DetailModel> _logger;

        public Pages_Client_Product_DetailModel(ShofyContext context, ILogger<Pages_Client_Product_DetailModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Product Product { get; set; } = null!;
        public IList<Review> Reviews { get; set; } = new List<Review>();
        public IList<Product> RelatedProducts { get; set; } = new List<Product>();

        public async Task<IActionResult> OnGetAsync(int? productId)
        {
            if (!productId.HasValue)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToPage("/Client/Product");
            }

            var product = await _context.Product
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.ProductID == productId.Value && p.Status == "Active");

            if (product == null)
            {
                TempData["Error"] = "Product not found or not available.";
                return RedirectToPage("/Client/Product");
            }

            Product = product;
            Reviews = Product.Reviews?.ToList() ?? new List<Review>();
            RelatedProducts = await _context.Product
                .Where(p => p.Status == Product.Status && p.ProductID != Product.ProductID && p.Status == "Active")
                .Take(8)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity)
        {
            var product = await _context.Product
                .FirstOrDefaultAsync(p => p.ProductID == productId && p.Status == "Active");
            if (product == null)
            {
                TempData["Error"] = "Product not found or not available.";
                return RedirectToPage();
            }

            if (quantity > product.StockQuantity)
            {
                TempData["Error"] = "Requested quantity exceeds available stock.";
                return RedirectToPage();
            }

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Please log in to add items to cart.";
                return RedirectToPage("/Accounts/Login");
            }

            var cart = await _context.Cart
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserID == userId.Value);

            if (cart == null)
            {
                cart = new Cart { UserID = userId.Value, CreatedAt = DateTime.Now };
                _context.Cart.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.CartItems?.FirstOrDefault(ci => ci.ProductID == productId);
            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartID = cart.CartID,
                    ProductID = productId,
                    Quantity = quantity
                };
                _context.CartItem.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            await _context.SaveChangesAsync();
            TempData["CartSuccess"] = $"{product.Name} is added to cart!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddReviewAsync(int productId, int rating, string comment)
        {
            if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Invalid review data.";
                return RedirectToPage();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(p => p.ProductID == productId && p.Status == "Active");
            if (product == null)
            {
                TempData["Error"] = "Product not found or not available.";
                return RedirectToPage();
            }

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Please log in to submit a review.";
                return RedirectToPage("/Accounts/Login");
            }

            var newReview = new Review
            {
                ProductID = productId,
                UserID = userId.Value,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.Review.Add(newReview);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Review submitted successfully!";
            return RedirectToPage();
        }
    }
}