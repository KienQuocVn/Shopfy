using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shofy.Pages.Client
{
    public class Pages_Client_ProductModel : PageModel
    {
        private readonly ShofyContext _context;
        private readonly ILogger<Pages_Client_ProductModel> _logger;

        public Pages_Client_ProductModel(ShofyContext context, ILogger<Pages_Client_ProductModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Thuộc tính để lưu danh sách sản phẩm
        public IList<Product> Products { get; set; } = new List<Product>();

        // Xử lý yêu cầu GET
        public async Task OnGetAsync(string? category = null, string? search = null, string? sort = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            // Truy vấn cơ bản lấy tất cả sản phẩm
            var query = _context.Product.AsQueryable();

            // Lọc theo danh mục (category)
            if (!string.IsNullOrEmpty(category))
            {
                // Giả sử danh mục được lưu trong một thuộc tính của Product, ví dụ: Tags
                query = query.Where(p => p.Status.Contains(category));
            }

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            // Sắp xếp
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort.ToLower())
                {
                    case "popularity":
                        // Giả sử sắp xếp theo số lượng đánh giá
                        query = query.OrderByDescending(p => p.Reviews != null ? p.Reviews.Count : 0);
                        break;
                    case "rating":
                        query = query.OrderByDescending(p => p.Reviews != null && p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0);
                        break;
                    case "newness":
                        query = query.OrderByDescending(p => p.CreatedDate);
                        break;
                    case "price-low-to-high":
                        query = query.OrderBy(p => p.Price);
                        break;
                    case "price-high-to-low":
                        query = query.OrderByDescending(p => p.Price);
                        break;
                    default:
                        query = query.OrderBy(p => p.ProductID);
                        break;
                }
            }

            // Lọc theo giá
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Lấy danh sách sản phẩm
            Products = await query.ToListAsync();
        }
    }
}