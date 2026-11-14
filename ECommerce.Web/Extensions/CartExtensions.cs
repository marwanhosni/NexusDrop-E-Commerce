namespace ECommerce.Web.Extensions;

public static class CartExtensions
{
    private const string CartCookieName = ".MarwanHosnyStore.CartId";

    public static string GetOrCreateCartId(this HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue(CartCookieName, out var cartId) && !string.IsNullOrWhiteSpace(cartId))
        {
            return cartId;
        }

        var newCartId = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        };

        httpContext.Response.Cookies.Append(CartCookieName, newCartId, cookieOptions);
        return newCartId;
    }
}

