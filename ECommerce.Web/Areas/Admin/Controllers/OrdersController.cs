using ECommerce.Application.Interfaces;
using ECommerce.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
    {
        await _orderService.UpdateOrderStatusAsync(orderId, status);
        TempData["SuccessMessage"] = $"Order #{orderId} status set to '{status}'.";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePaymentStatus(int orderId, PaymentStatus status)
    {
        await _orderService.UpdatePaymentStatusAsync(orderId, status);
        TempData["SuccessMessage"] = $"Order #{orderId} payment status updated to '{status}'.";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }
}

