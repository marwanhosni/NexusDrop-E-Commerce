using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Core.Entities;

public class CartItem
{
    public int Id { get; set; }

    public string CartId { get; set; } = string.Empty;
    public Cart Cart { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

