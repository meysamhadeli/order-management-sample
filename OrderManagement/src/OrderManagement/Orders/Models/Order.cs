using BuildingBlocks.Core.Model;
using BuildingBlocks.Exception;
using OrderManagement.Customers.Models;
using OrderManagement.Orders.Enums;

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
        if (!items.Any()) throw new DomainException("Order must have at least one item");

        return new Order
        {
            Id = id,
            CustomerId = customerId,
            OrderDate = orderDate ?? DateTime.UtcNow,
            OrderItems = items.ToList()
                       .AsReadOnly(),
            Status = OrderStatus.Pending
        };
    }

    // Domain command methods (return new instances)
    public Order AddItem(OrderItem newItem)
    {
        var existingItem = OrderItems.FirstOrDefault(i => i.Product == newItem.Product);

        var updatedItems = existingItem != null ?
                               OrderItems.Select(i =>
                                                     i.Product == newItem.Product ?
                                                         i with
                                                         {
                                                             Quantity =
                                                             i.Quantity + newItem.Quantity
                                                         } :
                                                         i)
                                   .ToList() :
                               OrderItems.Append(newItem).ToList();

        return this with {OrderItems = updatedItems.AsReadOnly()};
    }

    public Order RemoveItem(Guid itemId)
    {
        var updatedItems = OrderItems
            .Where(i => i.Id != itemId)
            .ToList()
            .AsReadOnly();

        if (updatedItems.Count == OrderItems.Count)
            throw new DomainException($"Item {itemId} not found in order");

        return this with {OrderItems = updatedItems};
    }

    public Order UpdateStatus(OrderStatus newStatus)
    {
        // Add status transition validation if needed
        return this with {Status = newStatus};
    }
}
