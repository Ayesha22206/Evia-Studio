using Evia.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Evia.Web.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /Shop?category=Men&style=Casual&q=shirt&color=Black&sort=price_asc
        public async Task<IActionResult> Index(string? category, string? style, string? color, string? size, decimal? minPrice, decimal? maxPrice, string? sort, string? q)
        {
            var queryable = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category) && category != "All")
                queryable = queryable.Where(p => p.Category == category);

            if (!string.IsNullOrWhiteSpace(style) && style != "All")
                queryable = queryable.Where(p => p.Style == style);

            if (!string.IsNullOrWhiteSpace(color) && color != "All")
                queryable = queryable.Where(p => p.Color == color);

            if (!string.IsNullOrWhiteSpace(size) && size != "All")
                queryable = queryable.Where(p => p.AvailableSizes.Contains(size));

            if (minPrice.HasValue)
                queryable = queryable.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                queryable = queryable.Where(p => p.Price <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(q))
                queryable = queryable.Where(p => p.Name.Contains(q) || (p.Description != null && p.Description.Contains(q)));

            queryable = sort switch
            {
                "price_asc" => queryable.OrderBy(p => p.Price),
                "price_desc" => queryable.OrderByDescending(p => p.Price),
                "popular" => queryable.OrderByDescending(p => p.StockQuantity),
                _ => queryable.OrderByDescending(p => p.CreatedAt)
            };

            ViewBag.Category = category;
            ViewBag.Style = style;
            ViewBag.Color = color;
            ViewBag.Size = size;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Sort = sort;
            ViewBag.Query = q;

            var products = await queryable.ToListAsync();
            return View(products);
        }

        // GET /Shop/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var relatedProducts = await _context.Products
                .Where(p => p.Category == product.Category && p.Id != product.Id)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;
            return View(product);
        }

        // GET /Shop/SearchSuggestions?q=hoodie
        public async Task<IActionResult> SearchSuggestions(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(new List<object>());

            var results = await _context.Products
                .Where(p => p.Name.Contains(q) || p.Category.Contains(q) || p.Style.Contains(q))
                .Take(5)
                .Select(p => new { id = p.Id, name = p.Name, price = p.Price, category = p.Category, imageUrl = p.ImageUrl })
                .ToListAsync();

            return Json(results);
        }
    }
}

