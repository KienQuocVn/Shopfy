using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Helpers;
using Shofy.Models;
using Shofy.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Shofy.Pages.Client
{
    public class Pages_Client_Shoping_CartModel : PageModel
    {
        private readonly ILogger<Pages_Client_Shoping_CartModel> _logger;
        private readonly ShofyContext _context;

        public Pages_Client_Shoping_CartModel(ILogger<Pages_Client_Shoping_CartModel> logger, ShofyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public List<CartItem> Cart { get; set; } = new();
        public decimal Total { get; set; }

        public void OnGet()
        {
            if (!HttpContext.Session.GetInt32("UserID").HasValue)
            {
                _logger.LogInformation("No user logged in, returning empty cart.");
                return;
            }

            Cart = HttpContext.Session.GetCart(_context);
            Total = Cart.Sum(item => item.Product.Price * item.Quantity);
            _logger.LogInformation("Cart loaded with {Count} items, Total: {Total}", Cart.Count, Total);
        }

        public IActionResult OnPostUpdateCart(int itemId, int quantity)
        {
            if (!HttpContext.Session.GetInt32("UserID").HasValue)
            {
                TempData["ErrorMessage"] = "Please log in to update cart.";
                return RedirectToPage("/Accounts/Login");
            }

            var cartItem = _context.CartItem
                .Include(ci => ci.Product)
                .FirstOrDefault(ci => ci.ItemID == itemId);

            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Cart item not found.";
                _logger.LogWarning("Cart item not found: {ItemId}", itemId);
                return RedirectToPage();
            }

            if (quantity > cartItem.Product.StockQuantity)
            {
                TempData["ErrorMessage"] = $"Only {cartItem.Product.StockQuantity} units of {cartItem.Product.Name} are available.";
                _logger.LogWarning("Quantity {Quantity} exceeds stock {Stock} for {Product}", quantity, cartItem.Product.StockQuantity, cartItem.Product.Name);
                return RedirectToPage();
            }

            if (quantity < 1)
            {
                quantity = 1;
            }

            cartItem.Quantity = quantity;
            _context.SaveChanges();
            _logger.LogInformation("Updated cart item {ItemId} to quantity {Quantity}", itemId, quantity);

            return RedirectToPage();
        }

        public IActionResult OnPostRemoveItem(int itemId)
        {
            if (!HttpContext.Session.GetInt32("UserID").HasValue)
            {
                TempData["ErrorMessage"] = "Please log in to remove items from cart.";
                return RedirectToPage("/Accounts/Login");
            }

            var cartItem = _context.CartItem.FirstOrDefault(ci => ci.ItemID == itemId);
            if (cartItem != null)
            {
                _context.CartItem.Remove(cartItem);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Item removed from cart.";
                _logger.LogInformation("Removed cart item {ItemId}", itemId);
            }
            else
            {
                TempData["ErrorMessage"] = "Cart item not found.";
                _logger.LogWarning("Cart item not found: {ItemId}", itemId);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostProceedToCheckout()
        {
            if (!HttpContext.Session.GetInt32("UserID").HasValue)
            {
                TempData["ErrorMessage"] = "Please log in to proceed to checkout.";
                return RedirectToPage("/Accounts/Login");
            }

            var cart = HttpContext.Session.GetCart(_context);
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                _logger.LogWarning("Empty cart during checkout attempt.");
                return RedirectToPage();
            }

            _logger.LogInformation("Proceeding to payment with {Count} items, Total: {Total}", cart.Count, Total);
            return RedirectToPage("/Client/Payment");
        }
    }
}