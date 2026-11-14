using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string cartId, string? userId = null);
    Task<CartDto> AddItemToCartAsync(string cartId, int productId, int quantity, string? userId = null);
    Task<CartDto> UpdateItemQuantityAsync(string cartId, int cartItemId, int newQuantity);
    Task<CartDto> RemoveItemAsync(string cartId, int cartItemId);
    Task ClearCartAsync(string cartId);
    Task MergeCartAsync(string guestCartId, string userId);
}

