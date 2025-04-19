using Shofy.Data;
using Shofy.Models;
using Shofy.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shofy.Pages.Client
{
    public class Pages_Client_PaymentModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<Pages_Client_PaymentModel> _logger;

        public Pages_Client_PaymentModel(ShofyContext context, ILogger<Pages_Client_PaymentModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<CartItem> CartItems { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public bool InvalidCartItems { get; set; } = false;

        [BindProperty]
        public string FullName { get; set; } = string.Empty;
        [BindProperty]
        public string Email { get; set; } = string.Empty;
        [BindProperty]
        public string Address { get; set; } = string.Empty;
        [BindProperty]
        public string PhoneNumber { get; set; } = string.Empty;
        [BindProperty]
        public string PaymentMethod { get; set; } = "card";

        public async Task OnGetAsync()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                _logger.LogInformation("No user logged in, redirecting to login.");
                return;
            }

            // Load CartItems with Product
            CartItems = await _context.CartItem
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart != null && ci.Cart.UserID == userId.Value)
                .ToListAsync();

            // Check for invalid CartItems (null Product)
            if (CartItems.Any(ci => ci.Product == null))
            {
                InvalidCartItems = true;
                _logger.LogWarning("Invalid cart items detected: Some products are missing.");
                CartItems = new List<CartItem>(); // Clear CartItems to avoid rendering issues
                TotalPrice = 0;
                return;
            }

            TotalPrice = CartItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity);
            _logger.LogInformation("Payment page loaded with {Count} items, Total: {Total}", CartItems.Count, TotalPrice);

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.UserID == userId.Value);
            if (user != null)
            {
                FullName = user.FullName ?? string.Empty;
                Email = user.Email;
                Address = user.Address ?? string.Empty;
                PhoneNumber = user.PhoneNumber ?? string.Empty;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Please log in to proceed with payment.";
                _logger.LogWarning("No user logged in during payment attempt.");
                return RedirectToPage("/Accounts/Login");
            }

            // Load CartItems with Product
            CartItems = await _context.CartItem
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart != null && ci.Cart.UserID == userId.Value)
                .ToListAsync();

            // Check for invalid CartItems (null Product)
            if (CartItems.Any(ci => ci.Product == null))
            {
                ModelState.AddModelError(string.Empty, "Some items in your cart are no longer available. Please review your cart.");
                _logger.LogWarning("Invalid cart items detected during payment.");
                InvalidCartItems = true;
                CartItems = new List<CartItem>();
                TotalPrice = 0;
                return Page();
            }

            if (!CartItems.Any())
            {
                ModelState.AddModelError(string.Empty, "Your cart is empty.");
                _logger.LogWarning("Empty cart during payment attempt.");
                TotalPrice = 0;
                return Page();
            }

            // Validate stock
            foreach (var item in CartItems)
            {
                var product = await _context.Product
                    .FirstOrDefaultAsync(p => p.ProductID == item.ProductID);
                if (product == null || product.StockQuantity < item.Quantity)
                {
                    ModelState.AddModelError(string.Empty, $"Sản phẩm '{item.Product?.Name ?? "Unknown"}' không đủ hàng! Còn lại: {product?.StockQuantity ?? 0}.");
                    _logger.LogWarning("Stock issue for {Product}: Requested {Quantity}, Available {Stock}", item.Product?.Name ?? "Unknown", item.Quantity, product?.StockQuantity ?? 0);
                    TotalPrice = CartItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity);
                    return Page();
                }
            }

            // Create Order
            var order = new Order
            {
                UserID = userId.Value,
                TotalPrice = CartItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity),
                Status = "Pending",
                PaymentMethod = PaymentMethod,
                OrderedDate = DateTime.Now,
                OrderDetails = new List<OrderDetail>()
            };

            foreach (var item in CartItems)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product?.Price ?? 0
                });
                // Update stock
                var product = await _context.Product
                    .FirstOrDefaultAsync(p => p.ProductID == item.ProductID);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            // Create Payment
            var payment = new Payment
            {
                Order = order,
                Amount = order.TotalPrice,
                Method = PaymentMethod,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            // Update User info
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.UserID == userId.Value);
            if (user != null)
            {
                user.FullName = FullName;
                user.Email = Email;
                user.Address = Address;
                user.PhoneNumber = PhoneNumber;
                _context.User.Update(user);
            }

            // Save to database
            _context.Order.Add(order);
            _context.Payment.Add(payment);
            _context.CartItem.RemoveRange(CartItems);
            await _context.SaveChangesAsync();

            HttpContext.Session.ClearCart(_context);
            _logger.LogInformation("Order {OrderId} created successfully, cart cleared.", order.OrderID);

            return RedirectToPage("/Client/ThankYou", new { orderId = order.OrderID });
        }
    }
}