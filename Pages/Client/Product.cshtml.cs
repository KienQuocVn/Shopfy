using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;
using Shofy.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shofy.Pages.Client
{
    public class Pages_Client_ProductModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<Pages_Client_ProductModel> _logger;

        public Pages_Client_ProductModel(ShofyContext context, ILogger<Pages_Client_ProductModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Product> Products { get; set; } = new List<Product>();
        public Product SelectedProduct { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PriceRange { get; set; }

        public List<int> WishlistProductIds { get; set; } = new List<int>();

        public async Task OnGetAsync(int? productId)
        {
            IQueryable<Product> productQuery = _context.Product;

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                productQuery = productQuery.Where(p => p.Name.Contains(SearchTerm));
            }

            if (!string.IsNullOrEmpty(PriceRange))
            {
                _logger.LogInformation($"Applying PriceRange filter: {PriceRange}");
                productQuery = FilterProductsByPriceRange(productQuery, PriceRange);
            }

            Products = await productQuery.ToListAsync();
            _logger.LogInformation($"Number of products after filter: {Products.Count}");

            if (productId.HasValue)
            {
                SelectedProduct = await _context.Product
                    .FirstOrDefaultAsync(p => p.ProductID == productId.Value);
            }

            var userId = HttpContext.Session.GetUserId();
            if (userId.HasValue)
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.UserID == userId.Value);
                if (user != null)
                {
                    WishlistProductIds = WishlistHelper.GetWishlistProductIds(user.Wishlist ?? "");
                }
            }
        }

        private IQueryable<Product> FilterProductsByPriceRange(IQueryable<Product> query, string priceRange)
        {
            return priceRange switch
            {
                "200000-500000" => query.Where(p => p.Price >= 200000 && p.Price <= 500000),
                "500000-1000000" => query.Where(p => p.Price > 500000 && p.Price <= 1000000),
                "1000000-1500000" => query.Where(p => p.Price > 1000000 && p.Price <= 1500000),
                "1500000-2000000" => query.Where(p => p.Price > 1500000 && p.Price <= 2000000),
                "2000000-2500000" => query.Where(p => p.Price > 2000000 && p.Price <= 2500000),
                _ => query, // Trường hợp không khớp, trả về toàn bộ sản phẩm
            };
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity = 1)
        {
            if (quantity <= 0) quantity = 1;

            var product = await _context.Product.FirstOrDefaultAsync(p => p.ProductID == productId && p.Status == "Active");
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToPage();
            }

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
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

            // Đảm bảo CartItems không null
            if (cart.CartItems == null)
            {
                cart.CartItems = new List<CartItem>();
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductID == productId);
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

        public async Task<IActionResult> OnPostToggleWishlistAsync(int productId)
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Please log in to add items to wishlist.";
                return RedirectToPage("/Accounts/Login");
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.UserID == userId.Value);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToPage();
            }

            var wishlist = WishlistHelper.GetWishlistProductIds(user.Wishlist ?? string.Empty);
            if (wishlist.Contains(productId))
            {
                // Xóa khỏi Wishlist
                await WishlistHelper.RemoveFromWishlistAsync(_context, userId.Value, productId);
                TempData["Success"] = "Product removed from wishlist!";
            }
            else
            {
                // Thêm vào Wishlist
                await WishlistHelper.AddToWishlistAsync(_context, userId.Value, productId);
                TempData["Success"] = "Product added to wishlist!";
            }

            return RedirectToPage();
        }
    }
}