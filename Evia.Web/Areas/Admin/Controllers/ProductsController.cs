using Evia.Web.Data;
using Evia.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Evia.Web.Areas.Admin.Controllers
{
    // Lets staff add/edit/remove the catalog items shown on the Shop page,
    // including uploading a real product photo (stored under wwwroot/images/products).
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET /Admin/Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(products);
        }

        // GET /Admin/Products/Create
        public IActionResult Create() => View(new Product());

        // POST /Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(product);

            if (imageFile is { Length: > 0 })
            {
                product.ImageUrl = await SaveImageAsync(imageFile);
            }

            product.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"\"{product.Name}\" was added to the catalog.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST /Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();
            if (!ModelState.IsValid) return View(product);

            var existing = await _context.Products.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Category = product.Category;
            existing.Style = product.Style;
            existing.Price = product.Price;
            existing.StockQuantity = product.StockQuantity;
            existing.IsFeatured = product.IsFeatured;

            if (imageFile is { Length: > 0 })
            {
                existing.ImageUrl = await SaveImageAsync(imageFile);
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = $"\"{existing.Name}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST /Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"\"{product.Name}\" was removed from the catalog.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Saves the uploaded file under wwwroot/images/products with a unique name
        // and returns the public, dynamic URL stored on the Product.
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                throw new InvalidOperationException("Only .jpg, .jpeg, .png, or .webp images are allowed.");

            var fileName = $"{Guid.NewGuid()}{ext}";
            var folder = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(folder);
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/products/{fileName}";
        }
    }
}
