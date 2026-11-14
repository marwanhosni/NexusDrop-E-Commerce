using ECommerce.Application.Interfaces;
using ECommerce.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public HomeController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllCategoriesAsync(activeOnly: true);
        var featuredProducts = await _productService.GetFeaturedProductsAsync(6);
        var latestProducts = (await _productService.GetProductsAsync(new Application.DTOs.ProductFilterDto { PageSize = 4 })).Items;

        var vm = new HomeViewModel
        {
            Categories = categories,
            FeaturedProducts = featuredProducts,
            LatestProducts = latestProducts
        };

        return View(vm);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
