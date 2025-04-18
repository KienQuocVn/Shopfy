using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Shofy.Pages.Client
{
    public class ProductDetailModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<ProductDetailModel> _logger;

        public ProductDetailModel(ShofyContext context, ILogger<ProductDetailModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Product? Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int? productId)
        {
            if (!productId.HasValue)
            {
                _logger.LogWarning("Không tìm thấy productId trong yêu cầu.");
                return NotFound();
            }

            // Truy vấn sản phẩm với đánh giá
            Product = await _context.Product
                .Include(p => p.Reviews) // Tải đánh giá
                .FirstOrDefaultAsync(p => p.ProductID == productId);

            if (Product == null)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm với ID: {ProductId}", productId);
                return NotFound();
            }

            _logger.LogInformation("Đã tải sản phẩm với ID: {ProductId}", productId);
            return Page();
        }
    }
}