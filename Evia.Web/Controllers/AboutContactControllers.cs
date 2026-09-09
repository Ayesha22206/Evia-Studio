using Evia.Web.Data;
using Evia.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Evia.Web.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index() => View();
    }

    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View(new ContactMessage());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactMessage contactMessage)
        {
            if (!ModelState.IsValid)
                return View(contactMessage);

            contactMessage.SentAt = DateTime.UtcNow;
            _context.ContactMessages.Add(contactMessage);
            await _context.SaveChangesAsync();

            ViewBag.Sent = true;
            ModelState.Clear(); // Clear the form input
            return View(new ContactMessage());
        }
    }
}
