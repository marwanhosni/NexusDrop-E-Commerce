using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CartDto> GetCartAsync(string cartId, string? userId = null)
    {
        var cart = await GetOrCreateCartInternalAsync(cartId, userId);
        return MapToDto(cart);
    }

    public async Task<CartDto> AddItemToCartAsync(string cartId, int productId, int quantity, string? userId = null)
    {
        if (quantity <= 0) quantity = 1;

        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null || !product.IsActive)
        {
            throw new InvalidOperationException("Product not found or is currently unavailable.");
        }

        var cart = await GetOrCreateCartInternalAsync(cartId, userId);

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            var newTotal = existingItem.Quantity + quantity;
            if (newTotal > product.StockQuantity)
            {
                existingItem.Quantity = product.StockQuantity;
            }
            else
            {
                existingItem.Quantity = newTotal;
            }
            existingItem.UnitPrice = product.Price;
        }
        else
        {
            var addQty = Math.Min(quantity, product.StockQuantity);
            if (addQty <= 0) addQty = 1; // allow adding even if backorder or standard stock

            cart.Items.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = addQty,
                UnitPrice = product.Price,
                AddedAt = DateTime.UtcNow
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.CompleteAsync();

        return await GetCartAsync(cart.Id, userId);
    }

    public async Task<CartDto> UpdateItemQuantityAsync(string cartId, int cartItemId, int newQuantity)
    {
        var cart = await GetOrCreateCartInternalAsync(cartId);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item != null)
        {
            if (newQuantity <= 0)
            {
                await _unitOfWork.CartItems.DeleteAsync(item);
            }
            else
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null && newQuantity > product.StockQuantity && product.StockQuantity > 0)
                {
                    item.Quantity = product.StockQuantity;
                }
                else
                {
                    item.Quantity = newQuantity;
                }
            }
            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.CompleteAsync();
        }

        return await GetCartAsync(cartId);
    }

    public async Task<CartDto> RemoveItemAsync(string cartId, int cartItemId)
    {
        var cart = await GetOrCreateCartInternalAsync(cartId);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item != null)
        {
            await _unitOfWork.CartItems.DeleteAsync(item);
            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.CompleteAsync();
        }

        return await GetCartAsync(cartId);
    }

    public async Task ClearCartAsync(string cartId)
    {
        var cart = await _unitOfWork.Carts.Query()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cartId);

        if (cart != null && cart.Items.Any())
        {
            foreach (var item in cart.Items.ToList())
            {
                await _unitOfWork.CartItems.DeleteAsync(item);
            }
            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.CompleteAsync();
        }
    }

    public async Task MergeCartAsync(string guestCartId, string userId)
    {
        if (string.IsNullOrWhiteSpace(guestCartId) || string.IsNullOrWhiteSpace(userId)) return;

        var guestCart = await _unitOfWork.Carts.Query()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == guestCartId);

        if (guestCart == null || !guestCart.Items.Any()) return;

        var userCart = await _unitOfWork.Carts.Query()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (userCart == null)
        {
            guestCart.UserId = userId;
            guestCart.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            foreach (var guestItem in guestCart.Items.ToList())
            {
                var existingUserItem = userCart.Items.FirstOrDefault(i => i.ProductId == guestItem.ProductId);
                if (existingUserItem != null)
                {
                    existingUserItem.Quantity += guestItem.Quantity;
                }
                else
                {
                    userCart.Items.Add(new CartItem
                    {
                        CartId = userCart.Id,
                        ProductId = guestItem.ProductId,
                        Quantity = guestItem.Quantity,
                        UnitPrice = guestItem.UnitPrice,
                        AddedAt = DateTime.UtcNow
                    });
                }
                await _unitOfWork.CartItems.DeleteAsync(guestItem);
            }
            userCart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Carts.DeleteAsync(guestCart);
        }

        await _unitOfWork.CompleteAsync();
    }

    private async Task<Cart> GetOrCreateCartInternalAsync(string cartId, string? userId = null)
    {
        Cart? cart = null;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            cart = await _unitOfWork.Carts.Query()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        if (cart == null && !string.IsNullOrWhiteSpace(cartId))
        {
            cart = await _unitOfWork.Carts.Query()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.Id == cartId);
        }

        if (cart == null)
        {
            cart = new Cart
            {
                Id = !string.IsNullOrWhiteSpace(cartId) ? cartId : Guid.NewGuid().ToString(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Carts.AddAsync(cart);
            await _unitOfWork.CompleteAsync();
        }

        return cart;
    }

    private static CartDto MapToDto(Cart cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Product",
                ProductSlug = i.Product?.Slug ?? string.Empty,
                ProductImageUrl = i.Product?.Images?.FirstOrDefault(img => img.IsPrimary)?.ImageUrl 
                                  ?? i.Product?.Images?.FirstOrDefault()?.ImageUrl 
                                  ?? "/images/products/placeholder.svg",
                CategoryName = i.Product?.Category?.Name ?? "Merchandise",
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                AvailableStock = i.Product?.StockQuantity ?? 0
            }).ToList()
        };
    }
}
