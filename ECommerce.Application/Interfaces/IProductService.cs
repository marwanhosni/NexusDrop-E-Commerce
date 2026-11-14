using ECommerce.Application.Common;
using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter);
    Task<IReadOnlyList<ProductDto>> GetFeaturedProductsAsync(int count = 6);
    Task<IReadOnlyList<ProductDto>> GetRelatedProductsAsync(int categoryId, int excludeProductId, int count = 4);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto?> GetBySlugAsync(string slug);
    Task<ProductDto> CreateProductAsync(ProductCreateUpdateDto dto);
    Task UpdateProductAsync(int id, ProductCreateUpdateDto dto);
    Task<bool> UpdateStockAsync(int id, int newStock);
    Task DeleteProductAsync(int id);
}

