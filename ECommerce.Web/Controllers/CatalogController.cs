using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class CatalogController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public CatalogController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    [HttpGet]
    [Route("catalog")]
    [Route("products")]
    [Route("category/{categorySlug}")]
    public async Task<IActionResult> Index([FromQuery] ProductFilterDto filter, string? categorySlug = null)
    {
        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            filter.CategorySlug = categorySlug;
        }

        var categories = await _categoryService.GetAllCategoriesAsync(activeOnly: true);
        var products = await _productService.GetProductsAsync(filter);

        CategoryDto? currentCat = null;
        if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            currentCat = categories.FirstOrDefault(c => c.Slug.Equals(filter.CategorySlug, StringComparison.OrdinalIgnoreCase));
        }
        else if (filter.CategoryId.HasValue)
        {
            currentCat = categories.FirstOrDefault(c => c.Id == filter.CategoryId.Value);
        }

        var vm = new CatalogViewModel
        {
            Products = products,
            Categories = categories,
            Filter = filter,
            CurrentCategory = currentCat
        };

        return View(vm);
    }

    [HttpGet]
    [Route("products/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return NotFound();
        }

        var product = await _productService.GetBySlugAsync(slug);
        if (product == null)
        {
            return NotFound();
        }

        var related = await _productService.GetRelatedProductsAsync(product.CategoryId, product.Id, count: 4);
        ViewBag.RelatedProducts = related;

        return View(product);
    }
}

