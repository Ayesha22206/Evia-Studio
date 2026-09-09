using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Evia.Web.Models
{
    // A ready-made catalog item shown in the Shop page
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required, StringLength(40)]
        public string Category { get; set; } = "Men"; // Men, Women, Kids, Unisex

        [Required, StringLength(40)]
        public string Style { get; set; } = "Casual"; // Casual, Formal, Hoodie, Frock, Oversized, Jacket

        [StringLength(40)]
        public string Color { get; set; } = "Black";

        [StringLength(100)]
        public string AvailableSizes { get; set; } = "S, M, L, XL";

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [StringLength(300)]
        public string? ImageUrl { get; set; }

        public int StockQuantity { get; set; } = 0;

        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // A customer-created outfit from the "Design Your Own" page
    public class CustomDesign
    {
        public int Id { get; set; }

        // Owner of the design; nullable so guests can also design before logging in
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required, StringLength(7)]
        public string Color { get; set; } = "#000000";

        [Required, StringLength(40)]
        public string Style { get; set; } = "Casual";

        [Required, StringLength(40)]
        public string Fabric { get; set; } = "Cotton";

        [Required, StringLength(40)]
        public string Pattern { get; set; } = "none";

        [Required, StringLength(10)]
        public string Size { get; set; } = "Small";

        [StringLength(200)]
        public string? CustomText { get; set; }

        [StringLength(7)]
        public string TextColor { get; set; } = "#FFFFFF";

        [Range(0, 200)]
        public double ChestMeasurement { get; set; }

        [Range(0, 200)]
        public double WaistMeasurement { get; set; }

        [Range(0, 200)]
        public double LengthMeasurement { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal EstimatedPrice { get; set; }

        // Populated after a successful call to the AI image generation service
        [StringLength(1000)]
        public string? GeneratedImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public List<OrderItem> Items { get; set; } = new();

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(300)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Cash on Delivery"; // Card, COD, JazzCash, EasyPaisa

        [StringLength(30)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed

        [StringLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Either a catalog Product or a CustomDesign can be ordered
        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? CustomDesignId { get; set; }
        public CustomDesign? CustomDesign { get; set; }

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }
    }

    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required, StringLength(50)]
        public string PaymentMethod { get; set; } = "CreditCard"; // CreditCard, COD, JazzCash, EasyPaisa

        [Required, StringLength(30)]
        public string PaymentStatus { get; set; } = "Completed"; // Pending, Completed, Failed, Refunded

        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ContactMessage
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}

