using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Features.Order;

namespace OrderManagementAPI.Persistance;

public class OrderManagementContext : DbContext
{
    public OrderManagementContext(DbContextOptions<OrderManagementContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
}