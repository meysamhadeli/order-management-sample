using BuildingBlocks.Core.Model;
using BuildingBlocks.Exception;
using OrderManagement.Customers.Models;
using OrderManagement.Orders.Dtos;
using OrderManagement.Orders.Enums;
using OrderManagement.Orders.Exceptions;
using OrderManagement.Orders.Features;

namespace OrderManagement.Orders.Models;

public record Order : Aggregate<Guid>
{
    public required Guid CustomerId { get; init; }
    public Customer? Customer { get; init; }
    public required DateTime OrderDate { get; init; } = DateTime.UtcNow;
    public required OrderStatus Status { get; init; } = OrderStatus.Pending;

    public required IReadOnlyCollection<OrderItem> OrderItems { get; init; } = new List<OrderItem>();

    public decimal TotalAmount => OrderItems.Sum(item => item.TotalPrice);

    // Private constructor for EF Core
    private Order() { }

    // Factory method with validation
    public static Order Create(
        Guid id,
        Guid customerId,
        IEnumerable<OrderItem> items,
        DateTime? orderDate = null
    )
    {
        if (!items.Any()) throw new InvalidOrderItemException();

        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            OrderDate = orderDate ?? DateTime.UtcNow,
            OrderItems = items.ToList()
                       .AsReadOnly(),
            Status = OrderStatus.Pending
        };

        order.AddDomainEvent(new OrderCreatedDomainEvent(
            order.Id,
            order.CustomerId,
            order.OrderDate,
            order.Status,
            order.TotalAmount,
            order.IsDeleted,
            order.OrderItems.Select(i => new OrderItemDto(i.Id, i.Product, i.UnitPrice, i.Quantity, i.TotalPrice)).ToList()));

        return order;
    }
}
