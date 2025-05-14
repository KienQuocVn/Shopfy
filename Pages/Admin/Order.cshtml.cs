using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System.IO;
using System.Threading.Tasks;

namespace Shofy.Pages.Admin
{
    public class OrderIndexModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<OrderIndexModel> _logger;

        // Number of orders per page
        public const int PageSize = 10;

        public OrderIndexModel(ShofyContext context, ILogger<OrderIndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // List of orders to display
        public List<Order> Orders { get; set; } = new();

        // Current page number
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        // Total number of pages
        public int TotalPages { get; set; }

        // Search keyword
        [BindProperty(SupportsGet = true)]
        public string Search { get; set; } = string.Empty;

        // Status filter
        [BindProperty(SupportsGet = true)]
        public string Status { get; set; }

        // Payment method filter
        [BindProperty(SupportsGet = true)]
        public string PaymentMethod { get; set; }


        // Check Role=Admin
        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToPage("/Error");
            }

            
            var query = _context.Order
                .Include(o => o.User)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(o =>
                    o.OrderID.ToString().Contains(Search) ||
                    o.Status.Contains(Search) ||
                    o.User.FullName.Contains(Search));
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(Status))
            {
                query = query.Where(o => o.Status == Status);
            }

            // Apply payment method filter
            if (!string.IsNullOrWhiteSpace(PaymentMethod))
            {
                query = query.Where(o => o.PaymentMethod == PaymentMethod);
            }

            // Sort by OrderID in ascending order (from 1 upwards)
            query = query.OrderBy(o => o.OrderID);

            // Get total number of orders matching filters
            var totalOrders = await query.CountAsync();

            // Calculate total pages based on PageSize
            TotalPages = (int)Math.Ceiling(totalOrders / (double)PageSize);

            // Get orders for the current page
            Orders = await query
                .Skip((CurrentPage - 1) * PageSize) // Skip orders from previous pages
                .Take(PageSize) // Take orders for the current page
                .ToListAsync();

            return Page();
        }

        // Delete an order
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToPage("/Error");
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage();
            }

            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Đã xóa đơn hàng ID: {id}");
            TempData["SuccessMessage"] = $"Đơn hàng ID {id} đã được xóa.";

            return RedirectToPage();
        }

        // Edit an order
        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToPage("/Error");
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage();
            }

            return RedirectToPage("/Admin/EditOrder", new { id });
        }

        // Export Invoice as PDF
        public async Task<IActionResult> OnGetExportInvoiceAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToPage("/Error");
            }

            // Fetch the order with related data (User and OrderDetails)
            var order = await _context.Order
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage();
            }

            using (var stream = new MemoryStream())
            {
                using (var writer = new PdfWriter(stream))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new Document(pdf);
                        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                        var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                        // Title
                        document.Add(new Paragraph("Invoice")
                            .SetFont(boldFont)
                            .SetFontSize(20)
                            .SetTextAlignment(TextAlignment.CENTER));

                        // Order Information
                        document.Add(new Paragraph($"Order ID: {order.OrderID}")
                            .SetFont(boldFont)
                            .SetFontSize(12)
                            .SetMarginTop(10));
                        document.Add(new Paragraph($"Customer: {order.User?.FullName ?? "N/A"}")
                            .SetFont(regularFont)
                            .SetFontSize(12));
                        document.Add(new Paragraph($"Ordered Date: {order.OrderedDate:dd/MM/yyyy HH:mm}")
                            .SetFont(regularFont)
                            .SetFontSize(12));
                        document.Add(new Paragraph($"Payment Method: {order.PaymentMethod}")
                            .SetFont(regularFont)
                            .SetFontSize(12));
                        document.Add(new Paragraph($"Status: {order.Status}")
                            .SetFont(regularFont)
                            .SetFontSize(12));

                        // Order Items Table
                        document.Add(new Paragraph("Order Items")
                            .SetFont(boldFont)
                            .SetFontSize(14)
                            .SetMarginTop(20));

                        var table = new Table(UnitValue.CreatePercentArray(new float[] { 40, 15, 15, 30 }))
                            .UseAllAvailableWidth();
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Product").SetFont(boldFont)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Quantity").SetFont(boldFont)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Unit Price").SetFont(boldFont)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Subtotal").SetFont(boldFont)));

                        foreach (var detail in order.OrderDetails ?? new List<OrderDetail>())
                        {
                            table.AddCell(new Cell().Add(new Paragraph(detail.Product?.Name ?? "N/A").SetFont(regularFont)));
                            table.AddCell(new Cell().Add(new Paragraph(detail.Quantity.ToString()).SetFont(regularFont)));
                            table.AddCell(new Cell().Add(new Paragraph($"${detail.UnitPrice:N2}").SetFont(regularFont)));
                            table.AddCell(new Cell().Add(new Paragraph($"${(detail.Quantity * detail.UnitPrice):N2}").SetFont(regularFont)));
                        }

                        document.Add(table);

                        // Total Price
                        document.Add(new Paragraph($"Total Price: ${order.TotalPrice:N2}")
                            .SetFont(boldFont)
                            .SetFontSize(14)
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetMarginTop(20));

                        // Footer
                        document.Add(new Paragraph($"Generated on: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .SetFont(regularFont)
                            .SetFontSize(10)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginTop(20));

                        document.Close();
                    }
                }

                var content = stream.ToArray();
                return File(content, "application/pdf", $"Invoice_Order_{order.OrderID}.pdf");
            }
        }
    }
}