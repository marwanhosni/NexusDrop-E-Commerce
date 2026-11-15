using ECommerce.Core.Entities;

namespace ECommerce.Application.Interfaces;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
}
