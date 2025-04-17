using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Helpers;
using Shofy.Models;
using Shofy.Data;
using System.Collections.Generic;

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
            Cart = HttpContext.Session.GetCart();
            Total = Cart.Sum(item => item.Product.Price * item.Quantity);
        }

        public IActionResult OnPostPayment()
        {
            // Lấy giỏ hàng từ session
            var cart = HttpContext.Session.GetCart();

            // Duyệt từng sản phẩm để giảm StockQuantity
            foreach (var item in cart)
            {
                var product = _context.Product.FirstOrDefault(p => p.ProductID == item.Product.ProductID);
                if (product != null && product.StockQuantity >= item.Quantity)
                {
                    product.StockQuantity -= item.Quantity;
                }
                else
                {
                    TempData["ErrorMessage"] = $"Sản phẩm '{item.Product.Name}' không đủ hàng!";
                    return RedirectToPage(); 
                }
            }


            // Lưu thay đổi
            _context.SaveChanges();

            // Xoá giỏ hàng sau khi thanh toán
            HttpContext.Session.ClearCart();

            // Chuyển hướng đến trang Thank You
            return RedirectToPage("/Client/ThankYou");
        }

    }
}