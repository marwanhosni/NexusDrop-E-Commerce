using ECommerce.Application.DTOs;

namespace ECommerce.Web.Models;

public class HomeViewModel
{
    public IReadOnlyList<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    public IReadOnlyList<ProductDto> FeaturedProducts { get; set; } = new List<ProductDto>();
    public IReadOnlyList<ProductDto> LatestProducts { get; set; } = new List<ProductDto>();
}

