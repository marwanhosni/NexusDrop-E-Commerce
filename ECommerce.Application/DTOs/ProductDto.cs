using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategorySlug { get; set; }
    public string PrimaryImageUrl { get; set; } = "/images/products/placeholder.svg";
    public List<ProductImageDto> Images { get; set; } = new();

    public bool InStock => StockQuantity > 0;
    public bool HasDiscount => OriginalPrice.HasValue && OriginalPrice > Price;
    public int DiscountPercentage => HasDiscount && OriginalPrice.HasValue && OriginalPrice > 0
        ? (int)Math.Round((1 - (Price / OriginalPrice.Value)) * 100)
        : 0;
}

public class ProductImageDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class ProductCreateUpdateDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 150 characters")]
    public string Name { get; set; } = string.Empty;

    public string? Slug { get; set; }

    [StringLength(300)]
    public string? ShortDescription { get; set; }

    [Required(ErrorMessage = "Full description is required")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 99999.99, ErrorMessage = "Price must be between 0.01 and 99999.99")]
    public decimal Price { get; set; }

    [Range(0.01, 99999.99, ErrorMessage = "Original price must be between 0.01 and 99999.99")]
    public decimal? OriginalPrice { get; set; }

    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, 100000, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; }

    public bool IsFeatured { get; set; } = false;

    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    public string? ImageUrl { get; set; }
}

public class ProductFilterDto
{
    public string? Query { get; set; }
    public int? CategoryId { get; set; }
    public string? CategorySlug { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; } // "price_asc", "price_desc", "newest", "featured"
    public bool? InStockOnly { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
