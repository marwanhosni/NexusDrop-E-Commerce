using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public ProductsController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ProductFilterDto filter)
    {
        filter.PageSize = 50; // Show up to 50 in admin
        var result = await _productService.GetProductsAsync(filter);
        var categories = await _categoryService.GetAllCategoriesAsync();

        ViewBag.Categories = categories;
        ViewBag.Filter = filter;

        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(new ProductCreateUpdateDto
        {
            StockQuantity = 20,
            IsActive = true,
            ImageUrl = "/images/products/placeholder.svg"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", dto.CategoryId);
            return View(dto);
        }

        await _productService.CreateProductAsync(dto);
        TempData["SuccessMessage"] = $"Product '{dto.Name}' was successfully deployed to the grid.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound();

        var categories = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);

        var dto = new ProductCreateUpdateDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Price = product.Price,
            OriginalPrice = product.OriginalPrice,
            SKU = product.SKU,
            StockQuantity = product.StockQuantity,
            IsFeatured = product.IsFeatured,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            ImageUrl = product.PrimaryImageUrl
        };

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductCreateUpdateDto dto)
    {
        if (id != dto.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", dto.CategoryId);
            return View(dto);
        }

        try
        {
            await _productService.UpdateProductAsync(id, dto);
            TempData["SuccessMessage"] = $"Product '{dto.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", dto.CategoryId);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteProductAsync(id);
        TempData["SuccessMessage"] = "Product has been archived/deactivated.";
        return RedirectToAction(nameof(Index));
    }
}

