using Evia.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Evia.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ProductCount = await _context.Products.CountAsync();
            ViewBag.DesignCount = await _context.CustomDesigns.CountAsync();
            ViewBag.OrderCount = await _context.Orders.CountAsync();
            ViewBag.MessageCount = await _context.ContactMessages.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders.Where(o => o.Status != "Cancelled").SumAsync(o => o.TotalAmount);
            
            ViewBag.RecentOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.LowStockProducts = await _context.Products
                .Where(p => p.StockQuantity < 10)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToListAsync();

            return View();
        }

        public async Task<IActionResult> Orders(string? status)
        {
            var queryable = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.Items).ThenInclude(i => i.CustomDesign)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                queryable = queryable.Where(o => o.Status == status);
            }

            ViewBag.CurrentStatus = status ?? "All";
            var orders = await queryable.OrderByDescending(o => o.CreatedAt).ToListAsync();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = newStatus;
                if (newStatus == "Delivered")
                {
                    order.PaymentStatus = "Paid";
                }
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Order #{orderId} status updated to '{newStatus}'.";
            }
            return RedirectToAction(nameof(Orders));
        }

        public async Task<IActionResult> CustomDesigns()
        {
            var designs = await _context.CustomDesigns
                .Include(d => d.User)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            return View(designs);
        }

        public async Task<IActionResult> Messages()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
            return View(messages);
        }
    }
}

