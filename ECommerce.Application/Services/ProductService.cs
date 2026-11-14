using ECommerce.Application.Common;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter)
    {
        var query = _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsNoTracking();

        // Active only by default for customer catalog
        query = query.Where(p => p.IsActive);

        // Search Query
        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var search = filter.Query.Trim().ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(search) || 
                (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(search)) ||
                p.Description.ToLower().Contains(search) ||
                p.SKU.ToLower().Contains(search));
        }

        // Category Filter
        if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            query = query.Where(p => p.Category.Slug.ToLower() == filter.CategorySlug.ToLower());
        }

        // Price Range
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }
        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        // In Stock Only
        if (filter.InStockOnly == true)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        // Sorting
        query = filter.SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "featured" => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt),
            "newest" or _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(filter.PageSize, 1, 50);
        var pageNumber = Math.Max(1, filter.PageNumber);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return new PagedResult<ProductDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyList<ProductDto>> GetFeaturedProductsAsync(int count = 6)
    {
        return await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ProductDto>> GetRelatedProductsAsync(int categoryId, int excludeProductId, int count = 4)
    {
        return await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsActive && p.CategoryId == categoryId && p.Id != excludeProductId)
            .OrderByDescending(p => p.IsFeatured)
            .Take(count)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug)
    {
        var product = await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Slug.ToLower() == slug.ToLower());

        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(ProductCreateUpdateDto dto)
    {
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? GenerateSlug(dto.Name)
            : GenerateSlug(dto.Slug);

        // Ensure unique slug
        var baseSlug = slug;
        var counter = 1;
        while (await _unitOfWork.Products.Query().AnyAsync(p => p.Slug == slug))
        {
            slug = $"{baseSlug}-{counter++}";
        }

        var product = new Product
        {
            Name = dto.Name,
            Slug = slug,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Price = dto.Price,
            OriginalPrice = dto.OriginalPrice,
            SKU = dto.SKU,
            StockQuantity = dto.StockQuantity,
            IsFeatured = dto.IsFeatured,
            IsActive = dto.IsActive,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            product.Images.Add(new ProductImage
            {
                ImageUrl = dto.ImageUrl,
                AltText = dto.Name,
                IsPrimary = true,
                DisplayOrder = 0
            });
        }

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.CompleteAsync();

        return MapToDto(product);
    }

    public async Task UpdateProductAsync(int id, ProductCreateUpdateDto dto)
    {
        var product = await _unitOfWork.Products.Query()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) throw new KeyNotFoundException($"Product with ID {id} not found.");

        product.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.Slug))
        {
            product.Slug = GenerateSlug(dto.Slug);
        }
        product.ShortDescription = dto.ShortDescription;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.OriginalPrice = dto.OriginalPrice;
        product.SKU = dto.SKU;
        product.StockQuantity = dto.StockQuantity;
        product.IsFeatured = dto.IsFeatured;
        product.IsActive = dto.IsActive;
        product.CategoryId = dto.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            var primary = product.Images.FirstOrDefault(i => i.IsPrimary);
            if (primary != null)
            {
                primary.ImageUrl = dto.ImageUrl;
            }
            else
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = dto.ImageUrl,
                    AltText = dto.Name,
                    IsPrimary = true
                });
            }
        }

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<bool> UpdateStockAsync(int id, int newStock)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return false;

        product.StockQuantity = Math.Max(0, newStock);
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product != null)
        {
            // Soft delete
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.CompleteAsync();
        }
    }

    private static ProductDto MapToDto(Product p)
    {
        var primaryImg = p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl 
                         ?? p.Images.FirstOrDefault()?.ImageUrl 
                         ?? "/images/products/placeholder.svg";

        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            ShortDescription = p.ShortDescription,
            Description = p.Description,
            Price = p.Price,
            OriginalPrice = p.OriginalPrice,
            SKU = p.SKU,
            StockQuantity = p.StockQuantity,
            IsFeatured = p.IsFeatured,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? "Uncategorized",
            CategorySlug = p.Category?.Slug,
            PrimaryImageUrl = primaryImg,
            Images = p.Images.OrderBy(i => i.DisplayOrder).Select(i => new ProductImageDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder
            }).ToList()
        };
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
