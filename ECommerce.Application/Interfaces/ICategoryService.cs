using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllCategoriesAsync(bool activeOnly = false);
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto?> GetBySlugAsync(string slug);
    Task<CategoryDto> CreateCategoryAsync(CategoryDto dto);
    Task UpdateCategoryAsync(int id, CategoryDto dto);
    Task DeleteCategoryAsync(int id);
}

