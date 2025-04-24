using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shofy.Data;
using Shofy.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Shofy.Pages.Admin
{
    public class EditProductModel : PageModel
    {
        private readonly ShofyContext _context;

        public EditProductModel(ShofyContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; }

        [BindProperty]
        public IFormFile ImageFile { get; set; }

        public IActionResult OnGet(int id)
        {
            Product = _context.Product.Find(id);
            if (Product == null)
            {
                return NotFound();
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            var productInDb = _context.Product.Find(Product.ProductID);
            if (productInDb == null)
            {
                return NotFound();
            }

            productInDb.Name = Product.Name;
            productInDb.Price = Product.Price;
            productInDb.StockQuantity = Product.StockQuantity;
            productInDb.Status = Product.Status;
            productInDb.Description = Product.Description;

            // Nếu có ảnh mới
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                var fileName = Path.GetFileName(ImageFile.FileName);
                var filePath = Path.Combine(uploads, fileName);
                var count = 1;

                // Tránh trùng tên
                while (System.IO.File.Exists(filePath))
                {
                    var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{count}{Path.GetExtension(fileName)}";
                    filePath = Path.Combine(uploads, newFileName);
                    count++;
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                productInDb.ImagePath = Path.GetFileName(filePath);
            }

            _context.SaveChanges();

            return RedirectToPage("Products");
        }
    }
}
