using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
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

        // Thuộc tính để lưu dữ liệu
        public Product Product { get; set; } = null!;
        public IList<Review> Reviews { get; set; } = new List<Review>();
        public IList<Product> RelatedProducts { get; set; } = new List<Product>();

        // Xử lý yêu cầu GET
        public async Task<IActionResult> OnGetAsync(int? productId)
        {
            if (!productId.HasValue)
            {
                return NotFound();
            }

            // Lấy thông tin sản phẩm cùng với Reviews
            var product = await _context.Product
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.ProductID == productId.Value);

            if (product == null)
            {
                return NotFound();
            }

            Product = product;

            if (Product == null)
            {
                return NotFound();
            }

            // Lấy danh sách đánh giá
            Reviews = Product.Reviews?.ToList() ?? new List<Review>();

            // Lấy sản phẩm liên quan (cùng danh mục, ví dụ: Status)
            RelatedProducts = await _context.Product
                .Where(p => p.Status == Product.Status && p.ProductID != Product.ProductID)
                .Take(8) // Giới hạn 8 sản phẩm
                .ToListAsync();

            return Page();
        }

        // Xử lý thêm vào giỏ hàng
        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity, string? size, string? color)
        {
            // Giả sử người dùng đã đăng nhập và bạn có UserID
            var userId = 1; // Thay bằng logic lấy UserID thực tế (ví dụ: từ UserManager)

            // Kiểm tra sản phẩm
            var product = await _context.Product.FindAsync(productId);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            // Kiểm tra giỏ hàng của người dùng
            var cart = await _context.Cart
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (cart == null)
            {
                cart = new Cart { UserID = userId, CreatedAt = DateTime.Now };
                _context.Cart.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
            var cartItem = await _context.CartItem
                .FirstOrDefaultAsync(ci => ci.CartID == cart.CartID && ci.ProductID == productId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartID = cart.CartID,
                    ProductID = productId,
                    Quantity = quantity,
                };
                _context.CartItem.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
                _context.CartItem.Update(cartItem);
            }

            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true, message = "Product added to cart!" });
        }

        // Xử lý gửi đánh giá
        public async Task<IActionResult> OnPostAddReviewAsync(int productId, int rating, string review, string name, string email)
        {
            if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(review) || string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Invalid review data.");
            }

            var product = await _context.Product.FindAsync(productId);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            var newReview = new Review
            {
                ProductID = productId,
                Rating = rating,
                Comment = review,
            };

            _context.Review.Add(newReview);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, message = "Review submitted successfully!" });
        }
    }
}