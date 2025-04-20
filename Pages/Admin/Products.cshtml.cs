using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;

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

        public async Task OnGetAsync(string? searchTerm, string? priceRange, int pageNumber = 1)
        {
            SearchTerm = searchTerm ?? string.Empty;
            PriceRange = priceRange ?? string.Empty;
            CurrentPage = pageNumber;

            var query = _context.Product.AsQueryable();

            // Lọc theo từ khóa
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(SearchTerm));
            }

            // Lọc theo khoảng giá
            query = PriceRange switch
            {
                "under100" => query.Where(p => p.Price < 100),
                "100to500" => query.Where(p => p.Price >= 100 && p.Price <= 500),
                "above500" => query.Where(p => p.Price > 500),
                _ => query
            };

            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            // Sắp xếp theo điều kiện
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
        }

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
    }
}
