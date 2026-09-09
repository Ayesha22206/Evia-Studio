using Evia.Web.Data;
using Evia.Web.Models;
using Evia.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Evia.Web.Controllers
{
    // Backs the "Design Your Own" page: customers pick fabric, color, pattern,
    // style and measurements, preview live, then save the design (per Abstract).
    public class DesignController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
       // private readonly IAiDesignService _aiDesignService;

        public DesignController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET /Design
        public IActionResult Index()
        {
            return View();
        }

        public class SaveDesignRequest
        {
            public string Color { get; set; } = "#000000";
            public string Style { get; set; } = "Casual";
            public string Fabric { get; set; } = "Cotton";
            public string Pattern { get; set; } = "none";
            public string Size { get; set; } = "Small";
            public string? CustomText { get; set; }
            public string TextColor { get; set; } = "#FFFFFF";
            public double Chest { get; set; }
            public double Waist { get; set; }
            public double Length { get; set; }
            public string? GeneratedImageUrl { get; set; }
        }

        public class AiPreviewRequest
        {
            public string Color { get; set; } = "#000000";
            public string Style { get; set; } = "Casual";
            public string Fabric { get; set; } = "Cotton";
            public string Pattern { get; set; } = "none";
            public string Size { get; set; } = "Small";
        }

        // POST /Design/GenerateAiPreview - turns the current selections into an
        // AI-generated outfit image (see Services/OpenAiDesignService.cs).
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> GenerateAiPreview([FromBody] AiPreviewRequest request, CancellationToken ct)
        //{
        //    var result = await _aiDesignService.GenerateOutfitImageAsync(
        //        new AiDesignRequest(request.Style, request.Fabric, request.Color, request.Pattern, request.Size), ct);

        //    if (!result.Success)
        //    {
        //        return BadRequest(new { success = false, message = result.ErrorMessage });
        //    }

        //    return Ok(new { success = true, imageUrl = result.ImageUrl });
        //}

        // POST /Design/Save  (called via jQuery $.ajax from the page)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] SaveDesignRequest request)
        {
            if (request.Chest <= 0 || request.Waist <= 0 || request.Length <= 0)
            {
                return BadRequest(new { success = false, message = "Please fill all required measurements first!" });
            }

            string? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                userId = user?.Id;
            }

            var design = new CustomDesign
            {
                UserId = userId,
                Color = request.Color,
                Style = request.Style,
                Fabric = request.Fabric,
                Pattern = request.Pattern,
                Size = request.Size,
                CustomText = request.CustomText,
                TextColor = request.TextColor,
                ChestMeasurement = request.Chest,
                WaistMeasurement = request.Waist,
                LengthMeasurement = request.Length,
                EstimatedPrice = EstimatePrice(request.Fabric, request.Style),
                GeneratedImageUrl = request.GeneratedImageUrl
            };

            _context.CustomDesigns.Add(design);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Your custom outfit design has been saved successfully!",
                designId = design.Id,
                estimatedPrice = design.EstimatedPrice
            });
        }

        // GET /Design/MyDesigns - lets a logged-in user view their saved designs
        public async Task<IActionResult> MyDesigns()
        {
            if (User.Identity?.IsAuthenticated != true)
                return Challenge();

            var user = await _userManager.GetUserAsync(User);
            var designs = await _context.CustomDesigns
                .Where(d => d.UserId == user!.Id)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(designs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return Challenge();

            var user = await _userManager.GetUserAsync(User);
            var design = await _context.CustomDesigns.FirstOrDefaultAsync(d => d.Id == id && d.UserId == user!.Id);
            if (design != null)
            {
                _context.CustomDesigns.Remove(design);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(MyDesigns));
        }

        // Simple base-price-plus-fabric-surcharge estimator; tune as needed.
        private static decimal EstimatePrice(string fabric, string style)
        {
            decimal basePrice = style switch
            {
                "Formal" => 6000,
                "Jacket" => 5500,
                "Frock" => 5000,
                "Hodie" or "Hoodie" => 4000,
                "Oversized" => 3800,
                _ => 3000
            };

            decimal fabricSurcharge = fabric switch
            {
                "Silk" => 2500,
                "Wool" => 2000,
                "Denim" => 1200,
                "Linen" => 900,
                _ => 0
            };

            return basePrice + fabricSurcharge;
        }
    }
}
