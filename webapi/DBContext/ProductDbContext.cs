using Microsoft.EntityFrameworkCore;
using webapi.Entities;

namespace webapi.DBContext
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //     modelBuilder.HasDefaultSchema("SYSTEM");
        //}

    }
}
