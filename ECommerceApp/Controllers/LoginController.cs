using Deneme7.Models;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly MyDbContext _context;
        private readonly EmailService _emailService;


        public LoginController(MyDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string Name, string LastName, string Email, string Password)
        {
            var user = new UserModel { Name = Name, LastName = LastName, Email = Email };

            bool check = await _context.Users.AnyAsync(u => u.Email == Email);

            if (check)
            {
                ModelState.AddModelError(string.Empty, "E-posta mevcut");
                return View();
            }

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(Password);
                byte[] hash = sha256.ComputeHash(bytes);
                user.Password = Convert.ToBase64String(hash);
            }

            user.Role = (user.Email == "admin@gmail.com" && Password == "12345") ? "Admin" : "User";

            user.ConfirmationToken = Guid.NewGuid().ToString();

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            var confirmationLink = Url.Action("ConfirmEmail", "Login", new { token = user.ConfirmationToken }, Request.Scheme);

            var subject = "Hesap Onayı";
            var message = $"Lütfen kaydınızı onaylamak için aşağıdaki bağlantıya tıklayın: <a href='{confirmationLink}'>Onayla</a>";
            await _emailService.SendEmailAsync(user.Email, subject, message);

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> ConfirmEmail(string token)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.ConfirmationToken == token);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            user.IsConfirmed = true; // Kullanıcıyı onayla


            await _context.SaveChangesAsync();

            return RedirectToAction("Login"); // Onaylandıktan sonra login ekranına yönlendir
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Login(string Email, string Password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null )
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı.");
                return View();
            }
            if(!user.IsConfirmed)
            {
                ModelState.AddModelError(string.Empty, "E-Posta Onaylanmamış");
                return View();
            }

            // Hash the password and compare with stored hash
            var hashedPassword = HashPassword(Password);
            if (user.Password == null || user.Password != hashedPassword)
            {
                ModelState.AddModelError(string.Empty, "Yanlış şifre.");
                return View();
            }


            // cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role) 
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);
            // bu kısmı iyice anla

            return RedirectToAction("Index", "Home");
        }





    }


}
