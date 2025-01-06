using Deneme7.Models;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace ECommerceApp.Controllers
{

    public class ProductController : Controller
    {
        private readonly MyDbContext _context;
        // Admin Girişi için
        private readonly UserManager<IdentityUser> _userManager; // login yapan kullanıcıyı buluyor
        private readonly CartService _cartService;

        // Sepeti Cache'de tutmak için 
        private readonly IMemoryCache _cache;
        private readonly string _cartCacheKey = "UserCart";

        private string GetCartCacheKeyForUser()
        {
            // Kullanıcı oturum açtıysa User.Identity.Name, değilse "Guest" olarak cache key oluştur
            var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest";
            return $"Cart_{userId}";
        }
        public ProductController(MyDbContext context, UserManager<IdentityUser> userManager, CartService cartService, IMemoryCache memoryCache)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
            _cache = memoryCache;
        }
        public IActionResult Index()
        {
            var list = _context.Products.ToList();
            var newlist = list.Where(u => u.IsDeleted == false).ToList();
            return View(newlist);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct(string Title, string Name, string Description, int Price, IFormFile ImageFile)
        {
            string imagePath = null;
            if (ImageFile != null)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                var filePath = Path.Combine(uploads, ImageFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                imagePath = "/images/products/" + ImageFile.FileName;
            }

            var product = new ProductModel
            {
                Title = Title,
                Name = Name,
                Description = Description,
                Price = Price,
                Image = imagePath
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult AddToCart()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int id)
        {
            // Ürünü veritabanından al
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(); // Ürün bulunamazsa 404 döndür
            }

            // Kullanıcıya özel cache anahtarını al
            var cartCacheKey = GetCartCacheKeyForUser();

            var cart = _cache.Get<List<CartModel>>(cartCacheKey) ?? new List<CartModel>();
            var cartItem = cart.FirstOrDefault(c => c.Id == product.Id);

            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                var cartProduct = new CartModel
                {
                    Id = product.Id,
                    Title = product.Title,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Quantity = 1,
                    Image = product.Image
                };
                cart.Add(cartProduct);
            }

            // Sepeti cache'e set et
            _cache.Set(cartCacheKey, cart, TimeSpan.FromMinutes(30)); // 30 dakika süreyle cache'de tut

            return RedirectToAction("Index", "Cart");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = _context.Products.Find(id);
            _context.Update(product);
            await _context.SaveChangesAsync();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductModel updatedProduct, IFormFile ImageFile)
        {
            var product = await _context.Products.FindAsync(updatedProduct.Id);
            if (product == null)
            {
                return NotFound();
            }

            product.Title = updatedProduct.Title;
            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Path.GetFileName(ImageFile.FileName);
                var uploadPath = Path.Combine("wwwroot/images/products", fileName);

                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                product.Image = $"/images/products/{fileName}";
            }

            // UserId'yi al
            var userId = _userManager.GetUserId(User); // Kullanıcı ID'sini alıyoruz
            product.SetUpdatedDate(userId); // SetUpdatedDate metodunu kullanarak güncelle

            _context.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult Delete()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            product.IsDeleted = true;
            _context.Update(product);
            await _context.SaveChangesAsync();


            return RedirectToAction("Index");
        }






    }
}