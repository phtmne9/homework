using Microsoft.EntityFrameworkCore;
using PvzApi.Entities;

namespace PvzApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<PickupPoint> PickupPoints => Set<PickupPoint>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Parcel> Parcels => Set<Parcel>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<PickupPoint>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.Address).IsRequired().HasMaxLength(200);
        });

        b.Entity<Client>(e =>
        {
            e.Property(c => c.FullName).IsRequired().HasMaxLength(150);
            e.Property(c => c.Phone).IsRequired().HasMaxLength(20);
            e.HasIndex(c => c.Phone).IsUnique();
        });

        b.Entity<Parcel>(e =>
        {
            // SQLite не поддерживает decimal в ORDER BY, поэтому храним как double
            e.Ignore(p => p.Deadline);
            e.Property(p => p.TrackNumber).IsRequired().HasMaxLength(50);
            e.HasIndex(p => p.TrackNumber).IsUnique();
            e.Property(p => p.Weight).HasConversion<double>();
            e.Property(p => p.Price).HasConversion<double>();
            e.Property(p => p.Status).HasConversion<int>();

            e.HasOne(p => p.PickupPoint)
                .WithMany(x => x.Parcels)
                .HasForeignKey(p => p.PickupPointId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Client)
                .WithMany(x => x.Parcels)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<User>(e =>
        {
            e.Property(u => u.Login).IsRequired().HasMaxLength(50);
            e.HasIndex(u => u.Login).IsUnique();
            e.Property(u => u.Role).IsRequired().HasMaxLength(30);
        });

        b.Entity<PickupPoint>().HasData(
            new PickupPoint { Id = 1, Name = "ПВЗ Абая", Address = "Алматы, Абая 150", Capacity = 300 },
            new PickupPoint { Id = 2, Name = "ПВЗ Сатпаева", Address = "Алматы, Сатпаева 22", Capacity = 150 });

        b.Entity<Client>().HasData(
            new Client { Id = 1, FullName = "Плисак Арсений", Phone = "+77011112233" },
            new Client { Id = 2, FullName = "Иванова Мария", Phone = "+77015556677" },
            new Client { Id = 3, FullName = "Ким Сергей", Phone = "+77018889900" });
    }
}
