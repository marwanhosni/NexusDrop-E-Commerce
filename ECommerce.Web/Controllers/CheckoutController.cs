using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Core.Entities;
using ECommerce.Web.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class CheckoutController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CheckoutController(
        ICartService cartService, 
        IOrderService orderService, 
        UserManager<ApplicationUser> userManager)
    {
        _cartService = cartService;
        _orderService = orderService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cartId = HttpContext.GetOrCreateCartId();
        var userId = _userManager.GetUserId(User);
        var cart = await _cartService.GetCartAsync(cartId, userId);

        if (!cart.Items.Any())
        {
            TempData["ErrorMessage"] = "Your cart is empty. Please select products to checkout.";
            return RedirectToAction("Index", "Cart");
        }

        ViewBag.Cart = cart;

        var checkoutDto = new CheckoutDto();

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                checkoutDto.CustomerName = user.FullName;
                checkoutDto.CustomerEmail = user.Email ?? string.Empty;
                checkoutDto.CustomerPhone = user.PhoneNumber ?? string.Empty;
                checkoutDto.ShippingAddress = user.StreetAddress ?? string.Empty;
                checkoutDto.City = user.City ?? string.Empty;
                checkoutDto.State = user.State ?? string.Empty;
                checkoutDto.PostalCode = user.PostalCode ?? string.Empty;
                checkoutDto.Country = string.IsNullOrWhiteSpace(user.Country) ? "United Neo Federation" : user.Country;
            }
        }

        return View(checkoutDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutDto model)
    {
        var cartId = HttpContext.GetOrCreateCartId();
        var userId = _userManager.GetUserId(User);
        var cart = await _cartService.GetCartAsync(cartId, userId);

        if (!cart.Items.Any())
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        ViewBag.Cart = cart;

        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        try
        {
            var order = await _orderService.CreateOrderFromCartAsync(cartId, model, userId);
            return RedirectToAction(nameof(Confirmation), new { orderNumber = order.OrderNumber });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Index", model);
        }
    }

    [HttpGet]
    [Route("checkout/confirmation/{orderNumber}")]
    public async Task<IActionResult> Confirmation(string orderNumber)
    {
        var order = await _orderService.GetOrderByNumberAsync(orderNumber);
        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }
}

