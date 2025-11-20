using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Features.Order;

namespace OrderManagementAPI.Persistance;

public class OrderManagementContext(DbContextOptions<OrderManagementContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
}