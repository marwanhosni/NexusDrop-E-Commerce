using ECommerce.Application.DTOs;
using ECommerce.Core.Enums;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderFromCartAsync(string cartId, CheckoutDto checkoutDto, string? userId = null);
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<OrderDto?> GetOrderByNumberAsync(string orderNumber);
    Task<IReadOnlyList<OrderDto>> GetOrdersByUserAsync(string userId);
    Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync();
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status);
}

