namespace ECommerce.Application.DTOs;

public class DashboardMetricsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int TotalCustomers { get; set; }
    public List<OrderDto> RecentOrders { get; set; } = new();
    public List<CategorySalesDto> CategoryBreakdown { get; set; } = new();
}

public class CategorySalesDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int ItemsSold { get; set; }
}

public class UserManagementDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int OrderCount { get; set; }
}

