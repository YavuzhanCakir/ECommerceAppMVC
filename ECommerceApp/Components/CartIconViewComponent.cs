using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

public class CartIconViewComponent : ViewComponent
{
    private readonly IMemoryCache _cache;

    public CartIconViewComponent(IMemoryCache cache)
    {
        _cache = cache;
    }
    public IViewComponentResult Invoke()
    {
        // Sepetteki ürün sayısını dinamik olarak alın. Örneğin:
        int itemCount = GetCartItemCount();

        return View(itemCount);
    }

    private int GetCartItemCount()
    {
        // Önbellekten sepet sayısını al
        if (!_cache.TryGetValue("CartItemCount", out int cartItemCount))
        {
            // Eğer önbellekte yoksa, varsayılan bir değer ver veya veritabanından çek
            cartItemCount = 0; // Örnek: 0 ya da veritabanından alınacak değer

            // Önce önbelleğe ekleyin
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)); // Önbellek süresi

            _cache.Set("CartItemCount", cartItemCount, cacheEntryOptions);
        }

        return cartItemCount;
    }

    public void UpdateCartItemCount(int newCount)
    {
        // Yeni sepet sayısını önbelleğe ekle
        _cache.Set("CartItemCount", newCount);
    }

}
