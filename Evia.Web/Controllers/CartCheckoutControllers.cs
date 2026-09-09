using Evia.Web.Data;
using Evia.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Evia.Web.Controllers
{
    // A lightweight session-based cart so visitors can add catalog products
    // or saved custom designs before heading to Checkout.
    public class CartItemDto
    {
        public string Type { get; set; } = "Product"; // "Product" or "CustomDesign"
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public string? ImageUrl { get; set; }
    }

    public class CartController : Controller
    {
        private const string CartSessionKey = "EviaCart";
        private const string PromoSessionKey = "EviaPromo";
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<CartItemDto> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(json)
                ? new List<CartItemDto>()
                : JsonSerializer.Deserialize<List<CartItemDto>>(json) ?? new List<CartItemDto>();
        }

        private void SaveCart(List<CartItemDto> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            ViewBag.PromoDiscount = HttpContext.Session.GetString(PromoSessionKey) == "EVIA10" ? 0.10m : 0.0m;
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.Type == "Product" && c.Id == productId);
            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.Add(new CartItemDto 
                { 
                    Type = "Product", 
                    Id = product.Id, 
                    Name = product.Name, 
                    Price = product.Price, 
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });

            SaveCart(cart);
            TempData["SuccessMessage"] = $"\"{product.Name}\" was added to your cart!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomDesign(int designId)
        {
            var design = await _context.CustomDesigns.FindAsync(designId);
            if (design == null) return NotFound();

            var cart = GetCart();
            cart.Add(new CartItemDto
            {
                Type = "CustomDesign",
                Id = design.Id,
                Name = $"Custom Outfit ({design.Style}, {design.Fabric}, {design.Size})",
                Price = design.EstimatedPrice,
                Quantity = 1,
                ImageUrl = design.GeneratedImageUrl ?? "/images/custom-outfit.jpg"
            });

            SaveCart(cart);
            TempData["SuccessMessage"] = "Your custom outfit was added to your cart!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(string type, int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.Type == type && c.Id == id);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(string type, int id)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.Type == type && c.Id == id);
            SaveCart(cart);
            TempData["InfoMessage"] = "Item removed from cart.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ApplyPromo(string promoCode)
        {
            if (!string.IsNullOrWhiteSpace(promoCode) && promoCode.Trim().ToUpper() == "EVIA10")
            {
                HttpContext.Session.SetString(PromoSessionKey, "EVIA10");
                TempData["SuccessMessage"] = "Promo code EVIA10 applied (10% OFF)!";
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid promo code. Try 'EVIA10' for 10% off.";
            }
            return RedirectToAction("Index");
        }
    }

    [Authorize]
    public class CheckoutController : Controller
    {
        private const string CartSessionKey = "EviaCart";
        private const string PromoSessionKey = "EviaPromo";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private List<CartItemDto> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(json)
                ? new List<CartItemDto>()
                : JsonSerializer.Deserialize<List<CartItemDto>>(json) ?? new List<CartItemDto>();
        }

        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index", "Cart");

            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserFullName = user?.FullName ?? string.Empty;
            ViewBag.UserAddress = user?.ShippingAddress ?? string.Empty;
            ViewBag.PromoDiscount = HttpContext.Session.GetString(PromoSessionKey) == "EVIA10" ? 0.10m : 0.0m;

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string shippingAddress, string paymentMethod = "Cash on Delivery")
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                ModelState.AddModelError("", "Your shopping cart is empty.");
                return View("Index", cart);
            }

            if (string.IsNullOrWhiteSpace(shippingAddress))
            {
                ModelState.AddModelError("", "Please enter a valid shipping address.");
                return View("Index", cart);
            }

            var user = await _userManager.GetUserAsync(User);
            
            decimal subtotal = cart.Sum(c => c.Price * c.Quantity);
            decimal discountRate = HttpContext.Session.GetString(PromoSessionKey) == "EVIA10" ? 0.10m : 0.0m;
            decimal totalAmount = subtotal * (1 - discountRate);

            var order = new Order
            {
                UserId = user!.Id,
                ShippingAddress = shippingAddress,
                TotalAmount = totalAmount,
                PaymentMethod = paymentMethod,
                PaymentStatus = paymentMethod == "Cash on Delivery" ? "Pending" : "Paid",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            foreach (var item in cart)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.Type == "Product" ? item.Id : null,
                    CustomDesignId = item.Type == "CustomDesign" ? item.Id : null,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create corresponding Payment audit entry
            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = totalAmount,
                PaymentMethod = paymentMethod,
                PaymentStatus = order.PaymentStatus,
                TransactionId = $"EVIA-TXN-{Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()}",
                CreatedAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);

            // Update user shipping address default if not present
            if (string.IsNullOrWhiteSpace(user.ShippingAddress))
            {
                user.ShippingAddress = shippingAddress;
                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();

            // Clear session cart & promo
            HttpContext.Session.Remove(CartSessionKey);
            HttpContext.Session.Remove(PromoSessionKey);

            return RedirectToAction("Confirmation", new { id = order.Id });
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.Items).ThenInclude(i => i.CustomDesign)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == id);
            ViewBag.Payment = payment;

            return View(order);
        }
    }
}

