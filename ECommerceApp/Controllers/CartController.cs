using Deneme7.Models;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerceApp.Controllers
{
    public class CartController : Controller
    {
        private readonly MyDbContext _context;

        private readonly IMemoryCache _cache;
        private readonly string _cartCacheKey = "UserCart";


        public CartController(MyDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _cache = memoryCache;   
        }

        private string GetCartCacheKeyForUser()
        {
            // Kullanıcı oturum açtıysa User.Identity.Name, değilse "Guest" olarak cache key oluştur
            var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest";
            return $"Cart_{userId}";
        }


        public async Task<IActionResult> Index()
        {
            // Kullanıcıya özel cache anahtarını al
            var cartCacheKey = GetCartCacheKeyForUser();

            // Sepeti cache'den al
            var cart = _cache.Get<List<CartModel>>(cartCacheKey) ?? new List<CartModel>();

            // Sepet görünümünü döndür
            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            // Burada veritabanında ilgili ürünü bul ve quantity değerini güncelle
            var product = _context.Cart.Find(id);
            if (product != null)
            {
                product.Quantity += quantity; // Veya direkt olarak quantity değerini güncelle
                _context.SaveChanges(); // Değişiklikleri kaydet
            }

            // Güncellenen ürünleri tekrar getir
            var updatedCart = _context.Cart.ToList(); // veya gerekli filtre ile

            return View("CartView", updatedCart); // İlgili view'ı döndür
        }

        public IActionResult Delete()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Delete(int id)
        {
            var cart = _cache.Get<List<CartModel>>(_cartCacheKey);

            if (cart != null)
            {
                // Sepette ürünü bul ve çıkar
                var cartItem = cart.FirstOrDefault(c => c.Id == id);

                if (cartItem != null)
                {
                    cart.Remove(cartItem);
                    _cache.Set(_cartCacheKey, cart, TimeSpan.FromMinutes(30)); // Cache'i güncelle
                }
            }

            return RedirectToAction("Index", "Cart");

        }

        



    }
}
