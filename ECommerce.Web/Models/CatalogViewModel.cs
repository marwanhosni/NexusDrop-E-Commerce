using ECommerce.Application.Common;
using ECommerce.Application.DTOs;

namespace ECommerce.Web.Models;

public class CatalogViewModel
{
    public PagedResult<ProductDto> Products { get; set; } = new();
    public IReadOnlyList<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    public ProductFilterDto Filter { get; set; } = new();
    public CategoryDto? CurrentCategory { get; set; }
}

