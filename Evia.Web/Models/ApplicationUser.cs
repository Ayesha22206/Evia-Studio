using Microsoft.AspNetCore.Identity;

namespace Evia.Web.Models
{
    // Extends Identity's default user with a couple of profile fields used across the site
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? ShippingAddress { get; set; }
    }
}
