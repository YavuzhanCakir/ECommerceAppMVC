using Deneme7.Models;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Veritaban� ba�lant�s�n� ekleyelim
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity hizmetlerini ekleyelim
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // E-posta do�rulamas�n� ileride ekleyebilirsin
})
.AddEntityFrameworkStores<MyDbContext>();

builder.Services.AddMemoryCache();
builder.Services.AddTransient<CartService>();


// MVC ve Razor Pages hizmetlerini ekleyelim
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// EmailService s�n�f�n� ekleyin
builder.Services.AddTransient<EmailService>();

// Kimlik do�rulama ayarlar�n� ekleyelim
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giri� yapmayan kullan�c�lar i�in y�nlendirme
        options.LogoutPath = "/Account/Logout"; // ��k�� yolu
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // �erezin ge�erlilik s�resi
        options.SlidingExpiration = true; // �erezin her istekte yenilenmesi
    });

var app = builder.Build();

// HTTP istek boru hatt�n� yap�land�r
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Kimlik do�rulama middleware'ini ekliyoruz
app.UseAuthorization();  // Yetkilendirme middleware'ini ekliyoruz

// Endpoints yap�land�rmas�
app.MapRazorPages();  // Identity Razor Pages UI �al��acak
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
