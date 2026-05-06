using BeautySalonAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasDefaultValue("client");
        });

        mb.Entity<Service>(e =>
        {
            e.Property(s => s.Price).HasPrecision(10, 2);
        });

        mb.Entity<Order>(e =>
        {
            e.Property(o => o.Price).HasPrecision(10, 2);
            e.Property(o => o.Status).HasConversion<string>();
            e.HasOne(o => o.Service).WithMany(s => s.Orders).HasForeignKey(o => o.ServiceId);
            e.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId).IsRequired(false);
            e.HasOne(o => o.Stylist).WithMany().HasForeignKey(o => o.StylistId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
