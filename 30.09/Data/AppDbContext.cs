using CityJoinsApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityJoinsApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<City> Cities => Set<City>();

    public DbSet<Country> Countries => Set<Country>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Country>().Property(c => c.Name).IsRequired().HasMaxLength(100);

        b.Entity<City>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.HasOne(c => c.Country)
                .WithMany(c => c.Cities)
                .HasForeignKey(c => c.CountryId);
        });

        b.Entity<Country>().HasData(
            new Country { Id = 1, Name = "Казахстан" },
            new Country { Id = 2, Name = "Германия" });

        b.Entity<City>().HasData(
            new City { Id = 1, Name = "Алматы", Population = 2_200_000, CountryId = 1 },
            new City { Id = 2, Name = "Астана", Population = 1_400_000, CountryId = 1 },
            new City { Id = 3, Name = "Берлин", Population = 3_700_000, CountryId = 2 });
    }
}
