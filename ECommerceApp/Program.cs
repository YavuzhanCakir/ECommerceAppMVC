using Deneme7.Models;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Veritabaný baðlantýsýný ekleyelim
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity hizmetlerini ekleyelim
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // E-posta doðrulamasýný ileride ekleyebilirsin
})
.AddEntityFrameworkStores<MyDbContext>();

builder.Services.AddMemoryCache();
builder.Services.AddTransient<CartService>();


// MVC ve Razor Pages hizmetlerini ekleyelim
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// EmailService sýnýfýný ekleyin
builder.Services.AddTransient<EmailService>();

// Kimlik doðrulama ayarlarýný ekleyelim
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giriþ yapmayan kullanýcýlar için yönlendirme
        options.LogoutPath = "/Account/Logout"; // Çýkýþ yolu
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Çerezin geçerlilik süresi
        options.SlidingExpiration = true; // Çerezin her istekte yenilenmesi
    });

var app = builder.Build();

// HTTP istek boru hattýný yapýlandýr
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Kimlik doðrulama middleware'ini ekliyoruz
app.UseAuthorization();  // Yetkilendirme middleware'ini ekliyoruz

// Endpoints yapýlandýrmasý
app.MapRazorPages();  // Identity Razor Pages UI çalýþacak
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
