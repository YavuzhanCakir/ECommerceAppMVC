using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
namespace Deneme7.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {

        }

        public DbSet<UserModel> Users { get; set; }

        public DbSet<ProductModel> Products { get; set; }

        public DbSet<CartModel> Cart { get; set; }


    }
}
