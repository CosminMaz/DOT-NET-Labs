using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Features.Order;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OrderManagementAPI.Persistance;

public class OrderManagementContext(DbContextOptions<OrderManagementContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure decimal to string conversion for SQLite
        modelBuilder.Entity<Order>()
            .Property(o => o.Price)
            .HasConversion(new DecimalToStringConverter());

        // Ignore the IsAvailable property as it is a calculated property
        modelBuilder.Entity<Order>()
            .Ignore(o => o.IsAvailable);

        base.OnModelCreating(modelBuilder);
    }
}

// Custom converter for Decimal to String
public class DecimalToStringConverter : ValueConverter<decimal, string>
{
    public DecimalToStringConverter()
        : base(
            v => v.ToString(System.Globalization.CultureInfo.InvariantCulture),
            v => decimal.Parse(v, System.Globalization.CultureInfo.InvariantCulture))
    {
    }
}
