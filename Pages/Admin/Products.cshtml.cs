using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;
using ClosedXML.Excel;
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
    public class ProductModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<ProductModel> _logger;

        public ProductModel(ShofyContext context, ILogger<ProductModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Product> Products { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
        public string PriceRange { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        private const int PageSize = 10;

        [TempData]
        public string? StatusMessage { get; set; }

        // Profile section properties
        public string Username { get; set; }
        public string Role { get; set; }
        public string Avatar { get; set; }
        
        // Check admin role
        public async Task<IActionResult> OnGetAsync(string? searchTerm, string? priceRange, int pageNumber = 1)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToPage("/Error");
            }

            // Retrieve profile data from session
            Username = HttpContext.Session.GetString("Username") ?? "Guest";
            Role = HttpContext.Session.GetString("Role") ?? "Unknown";
            Avatar = HttpContext.Session.GetString("Avatar") ?? "/images/noavt.jpg";

            SearchTerm = searchTerm ?? string.Empty;
            PriceRange = priceRange ?? string.Empty;
            CurrentPage = pageNumber;

            var query = _context.Product.AsQueryable();

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(SearchTerm));
            }

            // Filter by price range
            query = PriceRange switch
            {
                "under100" => query.Where(p => p.Price < 100),
                "100to500" => query.Where(p => p.Price >= 100 && p.Price <= 500),
                "above500" => query.Where(p => p.Price > 500),
                _ => query
            };

            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            // Sort by condition
            if (!string.IsNullOrWhiteSpace(SearchTerm) || !string.IsNullOrWhiteSpace(PriceRange))
            {
                query = query.OrderByDescending(p => p.Price);
            }
            else
            {
                query = query.OrderBy(p => p.ProductID);
            }

            Products = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }

        // Handle product deletion
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                StatusMessage = "Không tìm thấy sản phẩm cần xóa.";
                return RedirectToPage();
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Đã xóa sản phẩm với ID: {id}");
            StatusMessage = $"Đã xóa sản phẩm: {product.Name}";
            return RedirectToPage(new { pageNumber = CurrentPage });
        }

        // Export to Excel
        public async Task<IActionResult> OnPostExportExcelAsync()
        {
            var query = _context.Product.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(SearchTerm));
            }

            query = PriceRange switch
            {
                "under100" => query.Where(p => p.Price < 100),
                "100to500" => query.Where(p => p.Price >= 100 && p.Price <= 500),
                "above500" => query.Where(p => p.Price > 500),
                _ => query
            };

            var products = await query.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Price";
                worksheet.Cell(1, 4).Value = "Stock";
                worksheet.Cell(1, 5).Value = "Created Date";
                worksheet.Cell(1, 6).Value = "Status";

                for (int i = 0; i < products.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = products[i].ProductID;
                    worksheet.Cell(i + 2, 2).Value = products[i].Name;
                    worksheet.Cell(i + 2, 3).Value = products[i].Price;
                    worksheet.Cell(i + 2, 4).Value = products[i].StockQuantity;
                    worksheet.Cell(i + 2, 5).Value = products[i].CreatedDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(i + 2, 6).Value = products[i].Status;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
                }
            }
        }

        // Export to PDF
        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            var query = _context.Product.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(SearchTerm));
            }

            query = PriceRange switch
            {
                "under100" => query.Where(p => p.Price < 100),
                "100to500" => query.Where(p => p.Price >= 100 && p.Price <= 500),
                "above500" => query.Where(p => p.Price > 500),
                _ => query
            };

            var products = await query.ToListAsync();

            using (var stream = new MemoryStream())
            {
                using (var writer = new PdfWriter(stream))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new Document(pdf);
                        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                        document.Add(new Paragraph("Product List").SetFont(boldFont).SetFontSize(20));

                        var table = new Table(UnitValue.CreatePercentArray(new float[] { 10, 30, 15, 15, 20, 10 })).UseAllAvailableWidth();
                        table.AddHeaderCell("ID");
                        table.AddHeaderCell("Name");
                        table.AddHeaderCell("Price");
                        table.AddHeaderCell("Stock");
                        table.AddHeaderCell("Created Date");
                        table.AddHeaderCell("Status");

                        foreach (var product in products)
                        {
                            table.AddCell(product.ProductID.ToString());
                            table.AddCell(product.Name);
                            table.AddCell($"${product.Price:N2}");
                            table.AddCell(product.StockQuantity.ToString());
                            table.AddCell(product.CreatedDate.ToString("yyyy-MM-dd"));
                            table.AddCell(product.Status);
                        }

                        document.Add(table);
                        document.Close();
                    }
                }

                var content = stream.ToArray();
                return File(content, "application/pdf", "Products.pdf");
            }
        }
    }
}