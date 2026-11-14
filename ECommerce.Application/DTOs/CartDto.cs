namespace ECommerce.Application.DTOs;

public class CartDto
{
    public string Id { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public int TotalItemCount => Items.Sum(i => i.Quantity);
    public decimal Subtotal => Items.Sum(i => i.TotalPrice);
    public decimal ShippingEstimate => Subtotal > 100 || Subtotal == 0 ? 0m : 15m;
    public decimal TaxEstimate => Math.Round(Subtotal * 0.08m, 2);
    public decimal Total => Subtotal + ShippingEstimate + TaxEstimate;
}

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int AvailableStock { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}

public class AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemDto
{
    public int CartItemId { get; set; }
    public int Quantity { get; set; }
}

