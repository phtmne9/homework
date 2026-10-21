using CityApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<City> Cities => Set<City>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<City>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.Property(c => c.Country).IsRequired().HasMaxLength(100);
        });
    }
}
