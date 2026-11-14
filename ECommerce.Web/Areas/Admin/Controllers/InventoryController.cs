using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class InventoryController : Controller
{
    private readonly IProductService _productService;

    public InventoryController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _productService.GetProductsAsync(new Application.DTOs.ProductFilterDto { PageSize = 100 });
        return View(result.Items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStock(int productId, int stockQuantity)
    {
        var success = await _productService.UpdateStockAsync(productId, stockQuantity);
        if (success)
        {
            TempData["SuccessMessage"] = $"Stock levels updated for product #{productId}.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update stock.";
        }
        return RedirectToAction(nameof(Index));
    }
}

