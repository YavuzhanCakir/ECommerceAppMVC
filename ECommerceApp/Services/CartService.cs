using ECommerceApp.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

public class CartService
{
    private readonly IMemoryCache _cache;
    private readonly string _cacheKey = "UserCart_";

    public CartService(IMemoryCache memoryCache)
    {
        _cache = memoryCache;
    }

    // Sepete ürün ekleme
    public void AddToCart(string userId, CartItem item)
    {
        var cacheKey = _cacheKey + userId;
        List<CartItem> cart;

        // Kullanıcı için var olan sepeti getir
        if (_cache.TryGetValue(cacheKey, out List<CartItem> existingCart))
        {
            cart = existingCart;
        }
        else
        {
            cart = new List<CartItem>();
        }

        cart.Add(item);
        _cache.Set(cacheKey, cart, TimeSpan.FromHours(2)); // 2 saat boyunca cache'de tutar
    }

    // Sepeti getirme
    public List<CartItem> GetCart(string userId)
    {
        var cacheKey = _cacheKey + userId;
        _cache.TryGetValue(cacheKey, out List<CartItem> cart);
        return cart ?? new List<CartItem>();
    }

    // Sepeti temizleme
    public void ClearCart(string userId)
    {
        var cacheKey = _cacheKey + userId;
        _cache.Remove(cacheKey);
    }
}
