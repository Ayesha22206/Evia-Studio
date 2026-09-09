using Evia.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Evia.Web.Data
{
    // EVIA database context. Inherits IdentityDbContext so login/registration
    // (Customers stakeholder requirement) is handled by ASP.NET Core Identity,
    // backed by SQL Server as specified in the proposal's tools table.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<CustomDesign> CustomDesigns { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CustomDesign>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.CustomDesign)
                .WithMany()
                .HasForeignKey(oi => oi.CustomDesignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed a small product catalog so the Shop page isn't empty out of the box
            builder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Classic Oversized Hoodie", Description = "Soft fleece hoodie with a relaxed fit.", Category = "Unisex", Style = "Oversized", Color = "Black", AvailableSizes = "S, M, L, XL", Price = 4500, ImageUrl = "/images/product1.jpg", StockQuantity = 50, IsFeatured = true, CreatedAt = new DateTime(2025, 1, 1) },
                new Product { Id = 2, Name = "Elegant Silk Frock", Description = "Flowing silk frock for formal occasions.", Category = "Women", Style = "Frock", Color = "Red", AvailableSizes = "S, M, L", Price = 8500, ImageUrl = "/images/product2.jpg", StockQuantity = 30, IsFeatured = true, CreatedAt = new DateTime(2025, 1, 1) },
                new Product { Id = 3, Name = "Denim Jacket", Description = "Durable denim jacket with a modern cut.", Category = "Men", Style = "Jacket", Color = "Blue", AvailableSizes = "M, L, XL", Price = 6200, ImageUrl = "/images/product3.jpg", StockQuantity = 40, IsFeatured = false, CreatedAt = new DateTime(2025, 1, 1) },
                new Product { Id = 4, Name = "Linen Casual Shirt", Description = "Breathable linen shirt for everyday wear.", Category = "Men", Style = "Casual", Color = "White", AvailableSizes = "S, M, L, XL", Price = 3200, ImageUrl = "/images/product4.jpg", StockQuantity = 60, IsFeatured = false, CreatedAt = new DateTime(2025, 1, 1) },
                new Product { Id = 5, Name = "Wool Formal Coat", Description = "Tailored wool coat for formal settings.", Category = "Women", Style = "Formal", Color = "Navy", AvailableSizes = "S, M, L", Price = 12000, ImageUrl = "/images/product5.jpg", StockQuantity = 20, IsFeatured = true, CreatedAt = new DateTime(2025, 1, 1) },
                new Product { Id = 6, Name = "Cotton Hoodie", Description = "Everyday cotton hoodie, soft and warm.", Category = "Unisex", Style = "Hoodie", Color = "Grey", AvailableSizes = "S, M, L, XL, XXL", Price = 4000, ImageUrl = "/images/product6.jpg", StockQuantity = 55, IsFeatured = false, CreatedAt = new DateTime(2025, 1, 1) }
            );
        }
    }
}
