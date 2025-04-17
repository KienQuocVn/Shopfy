using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;
using System.Linq;

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

        // Danh sách sản phẩm
        public List<Product> Products { get; set; } = new();

        // Chuỗi tìm kiếm
        public string SearchTerm { get; set; } = string.Empty;

        // Số trang hiện tại
        public int CurrentPage { get; set; } = 1;

        // Tổng số trang
        public int TotalPages { get; set; }

        // Kích thước trang
        private const int PageSize = 10;

        // Phương thức xử lý GET và tìm kiếm sản phẩm với phân trang
        public async Task OnGetAsync(string searchTerm, int pageNumber = 1)
        {
            // Lưu giá trị tìm kiếm vào biến SearchTerm
            SearchTerm = searchTerm;
            CurrentPage = pageNumber;

            // Lọc danh sách sản phẩm theo từ khóa tìm kiếm (nếu có)
            var query = _context.Product.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm));
            }

            // Tính toán tổng số trang
            var totalProducts = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalProducts / (double)PageSize);

            // Lấy danh sách sản phẩm cho trang hiện tại
            Products = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // Xóa sản phẩm
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Product with ID {id} has been deleted.");
            }

            return RedirectToPage();
        }
    }
}
