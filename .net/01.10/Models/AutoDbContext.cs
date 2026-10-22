using Microsoft.EntityFrameworkCore;

namespace AutoApi.Models;

public partial class AutoDbContext : DbContext
{
    public AutoDbContext(DbContextOptions<AutoDbContext> options) : base(options) { }

    public virtual DbSet<Auto> Autos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auto>(entity =>
        {
            entity.ToTable("Auto");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Producer).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Cost).HasColumnType("decimal(12, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
