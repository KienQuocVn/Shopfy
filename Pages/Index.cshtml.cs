using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;
using Shofy.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shofy.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ShofyContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Danh sách sản phẩm hiển thị
        public List<Product> Products { get; set; } = new List<Product>();

        // Thông tin sản phẩm cho Modal
        public Product SelectedProduct { get; set; }

        // Tham số tìm kiếm
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        // Tham số lọc giá
        [BindProperty(SupportsGet = true)]
        public string PriceRange { get; set; }

        // Danh sách sản phẩm trong wishlist
        public List<int> WishlistProductIds { get; set; } = new List<int>();

        // GET
        public async Task OnGetAsync(int? productId)
        {
            // Truy vấn sản phẩm
            IQueryable<Product> productQuery = _context.Product;

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                productQuery = productQuery.Where(p => p.Name.Contains(SearchTerm));
            }

            // Lọc theo khoảng giá
            if (!string.IsNullOrEmpty(PriceRange))
            {
                _logger.LogInformation($"Applying PriceRange filter: {PriceRange}");
                switch (PriceRange)
                {
                    case "200000-500000":
                        productQuery = productQuery.Where(p => p.Price >= 200000 && p.Price <= 500000);
                        break;
                    case "500000-1000000":
                        productQuery = productQuery.Where(p => p.Price > 500000 && p.Price <= 1000000);
                        break;
                    case "1000000-1500000":
                        productQuery = productQuery.Where(p => p.Price > 1000000 && p.Price <= 1500000);
                        break;
                    case "1500000-2000000":
                        productQuery = productQuery.Where(p => p.Price > 1500000 && p.Price <= 2000000);
                        break;
                    case "2000000-2500000":
                        productQuery = productQuery.Where(p => p.Price > 2000000 && p.Price <= 2500000);
                        break;
                }
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

            // Lấy danh sách sản phẩm
            Products = await productQuery.ToListAsync();

            // Nếu có productId, lấy thông tin sản phẩm cho Modal
            if (productId.HasValue)
            {
                SelectedProduct = await _context.Product
                    .FirstOrDefaultAsync(p => p.ProductID == productId.Value);
            }
        }

        // POST: Thêm vào giỏ hàng
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


        // POST: Toggle Wishlist
        public async Task<IActionResult> OnPostToggleWishlistAsync(int productId)
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // AJAX request: Return JSON response
                    return new JsonResult(new { success = false, message = "Please log in to use the wishlist." });
                }
                else
                {
                    // Regular request: Redirect to login page
                    TempData["Error"] = "Please log in to use the wishlist.";
                    return RedirectToPage("/Accounts/Login");
                }
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.UserID == userId.Value);
            if (user == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // AJAX request: Return JSON response
                    return new JsonResult(new { success = false, message = "User not found." });
                }
                else
                {
                    // Regular request: Redirect to the current page
                    TempData["Error"] = "User not found.";
                    return RedirectToPage();
                }
            }

            var wishlist = WishlistHelper.GetWishlistProductIds(user.Wishlist ?? "");

            string action;
            if (wishlist.Contains(productId))
            {
                wishlist.Remove(productId);
                action = "removed";
            }
            else
            {
                wishlist.Add(productId);
                action = "added";
            }

            user.Wishlist = WishlistHelper.UpdateWishlistCsv(wishlist);
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // AJAX request: Return JSON response
                return new JsonResult(new { success = true, action = action });
            }
            else
            {
                // Regular request: Redirect to the current page
                return RedirectToPage(new { SearchTerm = SearchTerm, PriceRange = PriceRange });
            }
        }


    }
}