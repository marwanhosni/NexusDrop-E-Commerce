using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs;

public class CategoryDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? IconClass { get; set; }

    [Range(0, 1000)]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public int ProductCount { get; set; }
}

