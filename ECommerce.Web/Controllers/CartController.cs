using ECommerce.Application.Interfaces;
using ECommerce.Core.Entities;
using ECommerce.Web.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(ICartService cartService, UserManager<ApplicationUser> userManager)
    {
        _cartService = cartService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cartId = HttpContext.GetOrCreateCartId();
        var userId = _userManager.GetUserId(User);

        var cart = await _cartService.GetCartAsync(cartId, userId);
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var cartId = HttpContext.GetOrCreateCartId();
        var userId = _userManager.GetUserId(User);

        try
        {
            var cart = await _cartService.AddItemToCartAsync(cartId, productId, quantity, userId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return Json(new
                {
                    success = true,
                    message = "Item successfully beamed to your cart!",
                    totalItems = cart.TotalItemCount,
                    cartTotal = cart.Total
                });
            }

            TempData["SuccessMessage"] = "Item added to cart!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return Json(new { success = false, message = ex.Message });
            }

            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        var cartId = HttpContext.GetOrCreateCartId();
        await _cartService.UpdateItemQuantityAsync(cartId, cartItemId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var cartId = HttpContext.GetOrCreateCartId();
        await _cartService.RemoveItemAsync(cartId, cartItemId);
        TempData["SuccessMessage"] = "Item removed from cart.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        var cartId = HttpContext.GetOrCreateCartId();
        await _cartService.ClearCartAsync(cartId);
        TempData["SuccessMessage"] = "Shopping cart cleared.";
        return RedirectToAction(nameof(Index));
    }
}

