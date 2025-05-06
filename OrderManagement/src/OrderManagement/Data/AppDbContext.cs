using BuildingBlocks.EFCore;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Customers.Models;
using OrderManagement.Invoices.Models;
using OrderManagement.Orders.Models;

namespace OrderManagement.Data;

public sealed class AppDbContext : AppDbContextBase
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(OrderManagementRoot).Assembly);
        builder.FilterSoftDeletedProperties();
        builder.ToSnakeCaseTables();
    }
}
