using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Core.Entities;
using ECommerce.Core.Enums;
using ECommerce.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<DashboardMetricsDto> GetMetricsAsync()
    {
        var orders = await _unitOfWork.Orders.Query()
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        var products = await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .ToListAsync();

        var usersCount = await _userManager.Users.CountAsync();

        var totalRevenue = orders.Where(o => o.PaymentStatus == PaymentStatus.Paid).Sum(o => o.TotalAmount);
        var totalOrders = orders.Count;
        var pendingOrders = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing);
        var totalProducts = products.Count;
        var lowStockProducts = products.Count(p => p.StockQuantity <= 5);

        var recentOrders = orders.Take(5).Select(o => new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerName = o.CustomerName,
            CustomerEmail = o.CustomerEmail,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            PaymentStatus = o.PaymentStatus,
            Items = o.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice
            }).ToList()
        }).ToList();

        var categories = await _unitOfWork.Categories.Query()
            .Include(c => c.Products)
            .ToListAsync();

        var categoryBreakdown = categories.Select(c => new CategorySalesDto
        {
            CategoryName = c.Name,
            ProductCount = c.Products.Count,
            ItemsSold = orders.SelectMany(o => o.Items).Where(i => c.Products.Any(p => p.Id == i.ProductId)).Sum(i => i.Quantity)
        }).ToList();

        return new DashboardMetricsDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            TotalProducts = totalProducts,
            LowStockProducts = lowStockProducts,
            TotalCustomers = usersCount,
            RecentOrders = recentOrders,
            CategoryBreakdown = categoryBreakdown
        };
    }

    public async Task<IReadOnlyList<UserManagementDto>> GetUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var orders = await _unitOfWork.Orders.Query().ToListAsync();

        var list = new List<UserManagementDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Customer";

            list.Add(new UserManagementDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role = role,
                CreatedAt = user.CreatedAt,
                OrderCount = orders.Count(o => o.UserId == user.Id)
            });
        }

        return list;
    }
}

