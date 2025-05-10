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

        public string Avatar { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

        // Load product information and user details from session
        public IActionResult OnGet(int id)
        {
            Product = _context.Product.Find(id);
            if (Product == null)
            {
                return NotFound();
            }

            // Get user info from session
            Username = HttpContext.Session.GetString("Username") ?? "Guest";
            Role = HttpContext.Session.GetString("Role") ?? "Unknown";
            Avatar = HttpContext.Session.GetString("Avatar") ?? "/images/noavt.jpg";

            return Page();
        }

        // Handle the form submission and update product details
        public IActionResult OnPost()
        {
            var productInDb = _context.Product.Find(Product.ProductID);
            if (productInDb == null)
            {
                return NotFound();
            }

            // Update product details
            productInDb.Name = Product.Name;
            productInDb.Price = Product.Price;
            productInDb.StockQuantity = Product.StockQuantity;
            productInDb.Status = Product.Status;
            productInDb.Description = Product.Description;

            // Handle image file upload
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

                // Avoid file name collision
                while (System.IO.File.Exists(filePath))
                {
                    var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{count}{Path.GetExtension(fileName)}";
                    filePath = Path.Combine(uploads, newFileName);
                    count++;
                }

                // Save the image to the server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                // Update product with new image path
                productInDb.ImagePath = Path.GetFileName(filePath);
            }

            // Save changes to the database
            _context.SaveChanges();

            return RedirectToPage("Products");
        }
    }
}
