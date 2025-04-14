using Shofy.Data;
using Shofy.Models;
using Shofy.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Shofy.Pages.Client;

public class Pages_Client_PaymentModel : PageModel
{
    private readonly ShofyContext _context;
    private readonly ILogger<Pages_Client_PaymentModel> _logger;

    public Pages_Client_PaymentModel(ShofyContext context,ILogger<Pages_Client_PaymentModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    public decimal TotalPrice { get; set; }

    [BindProperty]
    public required string FullName { get; set; }
    [BindProperty]
    public required string Email { get; set; }
    [BindProperty]
    public required string Address { get; set; }
    [BindProperty]
    public required string PhoneNumber { get; set; }
    [BindProperty]
    public required string PaymentMethod { get; set; }

    public async Task OnGetAsync()
    {
        var userID = HttpContext.Session.GetUserId();

        CartItems = await _context.CartItem
            .Include(ci => ci.Product)
            .Where(ci => ci.Cart != null && ci.Cart.UserID == userID)
            .ToListAsync();

        TotalPrice = CartItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userID = HttpContext.Session.GetUserId();

        CartItems = await _context.CartItem
            .Include(ci => ci.Product)
            .Where(ci => ci.Cart != null && ci.Cart.UserID == userID)
            .ToListAsync();

        if (!CartItems.Any())
        {
            ModelState.AddModelError(string.Empty, "Your cart is empty.");
            return Page();
        }

        var order = new Order
        {
            UserID = userID ?? throw new InvalidOperationException("User ID cannot be null."),
            TotalPrice = CartItems.Sum(item => (item.Product?.Price ?? 0) * item.Quantity),
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
        }

        _context.Order.Add(order);

        _context.CartItem.RemoveRange(CartItems);

        await _context.SaveChangesAsync();

        return RedirectToPage("/Client/Order", new { orderId = order.OrderID });
    }
}