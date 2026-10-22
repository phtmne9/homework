using Microsoft.EntityFrameworkCore;
using ProductApi.Entities;

namespace ProductApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Product>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.Price).HasPrecision(12, 2);
        });

        b.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Клавиатура", Price = 25000m, Quantity = 10 },
            new Product { Id = 2, Name = "Монитор", Price = 140000m, Quantity = 4 });
    }
}
