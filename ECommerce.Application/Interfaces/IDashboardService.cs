using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardMetricsDto> GetMetricsAsync();
    Task<IReadOnlyList<UserManagementDto>> GetUsersAsync();
}

