using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllCategoriesAsync(bool activeOnly = false)
    {
        var query = _unitOfWork.Categories.Query()
            .Include(c => c.Products);

        var list = activeOnly 
            ? await query.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync()
            : await query.OrderBy(c => c.DisplayOrder).ToListAsync();

        return list.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            IconClass = c.IconClass,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            ProductCount = c.Products.Count(p => p.IsActive)
        }).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.Query()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            IconClass = category.IconClass,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ProductCount = category.Products.Count
        };
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        var category = await _unitOfWork.Categories.Query()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Slug.ToLower() == slug.ToLower());

        if (category == null) return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            IconClass = category.IconClass,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ProductCount = category.Products.Count
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CategoryDto dto)
    {
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? GenerateSlug(dto.Name)
            : GenerateSlug(dto.Slug);

        var category = new Category
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            IconClass = dto.IconClass ?? "bi-tag",
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.CompleteAsync();

        dto.Id = category.Id;
        dto.Slug = category.Slug;
        return dto;
    }

    public async Task UpdateCategoryAsync(int id, CategoryDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null) throw new KeyNotFoundException($"Category with ID {id} was not found.");

        category.Name = dto.Name;
        category.Slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : GenerateSlug(dto.Slug);
        category.Description = dto.Description;
        category.IconClass = dto.IconClass;
        category.DisplayOrder = dto.DisplayOrder;
        category.IsActive = dto.IsActive;

        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _unitOfWork.Categories.Query()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category != null)
        {
            if (category.Products.Any())
            {
                // Soft delete by deactivating or prevent deletion
                category.IsActive = false;
                await _unitOfWork.Categories.UpdateAsync(category);
            }
            else
            {
                await _unitOfWork.Categories.DeleteAsync(category);
            }
            await _unitOfWork.CompleteAsync();
        }
    }

    private static string GenerateSlug(string text)
    {
        return text.Trim().ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("/", "-")
            .Replace("--", "-");
    }
}

