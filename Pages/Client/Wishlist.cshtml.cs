using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
using Shofy.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Shofy.Pages.Client
{
    public class WishlistModel : PageModel
    {
        private readonly ShofyContext _context;

        public WishlistModel(ShofyContext context)
        {
            _context = context;
        }

        public List<Product> WishlistProducts { get; set; } = new List<Product>();
        public List<int> WishlistProductIds { get; set; } = new List<int>();

        public async Task<IActionResult> OnGetAsync()
        {

            var userId = HttpContext.Session.GetUserId();
            if (userId.HasValue)
            {
                // Lấy người dùng từ cơ sở dữ liệu
                var user = await _context.User.FirstOrDefaultAsync(u => u.UserID == userId.Value);

                if (user != null)
                {
                    // Lấy danh sách sản phẩm yêu thích của người dùng từ Wishlist
                    WishlistProductIds = user.Wishlist != null 
                        ? WishlistHelper.GetWishlistProductIds(user.Wishlist) 
                        : new List<int>();
                    WishlistProducts = await WishlistHelper.GetWishlistProductsAsync(_context, userId.Value);
                }

                return Page();
            }
            else
            {
                TempData["Error"] = "Please log in to view your wishlist.";
                return RedirectToPage("/Accounts/Login");
            }


        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity = 1)
        {
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

            var wishlist = WishlistHelper.GetWishlistProductIds(user.Wishlist);
            if (wishlist.Contains(productId))
            {
                await WishlistHelper.RemoveFromWishlistAsync(_context, userId.Value, productId);
                TempData["Success"] = "Product removed from wishlist!";
            }
            else
            {
                await WishlistHelper.AddToWishlistAsync(_context, userId.Value, productId);
                TempData["Success"] = "Product added to wishlist!";
            }

            return RedirectToPage();
        }
        public async Task<IActionResult> OnGetWishlistCountAsync()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                return new JsonResult(0);
            }

            var wishlist = await WishlistHelper.GetWishlistProductsAsync(_context, userId.Value);
            return new JsonResult(wishlist.Count);
        }
    }
}