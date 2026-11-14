using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Core.Entities;
using ECommerce.Core.Enums;
using ECommerce.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartService _cartService;

    public OrderService(IUnitOfWork unitOfWork, ICartService cartService)
    {
        _unitOfWork = unitOfWork;
        _cartService = cartService;
    }

    public async Task<OrderDto> CreateOrderFromCartAsync(string cartId, CheckoutDto checkoutDto, string? userId = null)
    {
        var cart = await _cartService.GetCartAsync(cartId, userId);
        if (!cart.Items.Any())
        {
            throw new InvalidOperationException("Cannot checkout an empty cart.");
        }

        // Validate stock for all items
        foreach (var item in cart.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product == null || !product.IsActive)
            {
                throw new InvalidOperationException($"Product '{item.ProductName}' is no longer available.");
            }
            if (product.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for '{item.ProductName}'. Only {product.StockQuantity} available.");
            }
        }

        var orderNumber = $"MHS-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var subtotal = cart.Subtotal;
        var shipping = cart.ShippingEstimate;
        var tax = cart.TaxEstimate;
        var total = subtotal + shipping + tax;

        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            CustomerName = checkoutDto.CustomerName,
            CustomerEmail = checkoutDto.CustomerEmail,
            CustomerPhone = checkoutDto.CustomerPhone,
            ShippingAddress = checkoutDto.ShippingAddress,
            City = checkoutDto.City,
            State = checkoutDto.State,
            PostalCode = checkoutDto.PostalCode,
            Country = checkoutDto.Country,
            OrderDate = DateTime.UtcNow,
            Subtotal = subtotal,
            ShippingCost = shipping,
            TaxAmount = tax,
            TotalAmount = total,
            Status = OrderStatus.Processing,
            PaymentStatus = PaymentStatus.Paid,
            PaymentMethod = checkoutDto.PaymentMethod,
            Notes = checkoutDto.Notes
        };

        foreach (var item in cart.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductImageUrl = item.ProductImageUrl,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.TotalPrice
            });

            // Deduct stock
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
                product.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Products.UpdateAsync(product);
            }
        }

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.CompleteAsync();

        // Clear cart
        await _cartService.ClearCartAsync(cartId);

        return await GetOrderByIdAsync(order.Id) ?? throw new Exception("Failed to retrieve created order.");
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _unitOfWork.Orders.Query()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
    {
        var order = await _unitOfWork.Orders.Query()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber.ToLower() == orderNumber.ToLower());

        return order == null ? null : MapToDto(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersByUserAsync(string userId)
    {
        var orders = await _unitOfWork.Orders.Query()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _unitOfWork.Orders.Query()
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order != null)
        {
            order.Status = status;
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CompleteAsync();
        }
    }

    public async Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order != null)
        {
            order.PaymentStatus = status;
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CompleteAsync();
        }
    }

    private static OrderDto MapToDto(Order o)
    {
        return new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            UserId = o.UserId,
            CustomerName = o.CustomerName,
            CustomerEmail = o.CustomerEmail,
            CustomerPhone = o.CustomerPhone,
            ShippingAddress = o.ShippingAddress,
            City = o.City,
            State = o.State,
            PostalCode = o.PostalCode,
            Country = o.Country,
            OrderDate = o.OrderDate,
            Subtotal = o.Subtotal,
            ShippingCost = o.ShippingCost,
            TaxAmount = o.TaxAmount,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            PaymentStatus = o.PaymentStatus,
            PaymentMethod = o.PaymentMethod,
            Notes = o.Notes,
            Items = o.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductImageUrl = i.ProductImageUrl,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }
}

