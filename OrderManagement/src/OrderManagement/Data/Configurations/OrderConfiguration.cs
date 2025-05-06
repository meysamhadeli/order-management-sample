using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Customers.Models;
using OrderManagement.Orders.Models;

namespace OrderManagement.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(nameof(Order));

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.OrderDate)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Domain-calculated total (not stored in DB)
        builder.Ignore(o => o.TotalAmount);

        // Customer relationship (reference to external aggregate)
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrderItems owned collection configuration
        builder.OwnsMany(o => o.OrderItems, oi =>
        {
            oi.ToTable(nameof(OrderItem));

            oi.HasKey(oi => oi.Id);

            oi.Property(oi => oi.Product)
                .IsRequired()
                .HasMaxLength(200);

            oi.Property(oi => oi.UnitPrice)
                .IsRequired();

            oi.Property(oi => oi.Quantity)
                .IsRequired();

            // Domain-calculated total price (not stored in DB)
            oi.Ignore(oi => oi.TotalPrice);

            // Indexes
            oi.HasIndex(oi => oi.Product);
        });

        // Optimistic concurrency
        builder.Property(o => o.Version)
            .IsRowVersion()
            .IsRequired();
    }
}

