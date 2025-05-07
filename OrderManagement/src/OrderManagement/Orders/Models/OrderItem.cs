using BuildingBlocks.Exception;
using OrderManagement.Orders.Exceptions;
using OrderManagement.Orders.Features;

namespace OrderManagement.Orders.Models;

public record OrderItem
{
    public required Guid Id { get; init; }
    public required Guid OrderId { get; init; }
    public Order? Order { get; init; }
    public required string Product { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int Quantity { get; init; }
    public decimal TotalPrice => UnitPrice * Quantity;

    // Factory method
    public static OrderItem Create(
        Guid id,
        Guid orderId,
        string product,
        decimal unitPrice,
        int quantity = 1)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        if (unitPrice <= 0)
            throw new invalidUnitPriceException();

        var orderItem =  new OrderItem
               {
                   Id = id,
                   OrderId = orderId,
                   Product = product,
                   UnitPrice = unitPrice,
                   Quantity = quantity
               };

        return orderItem;
    }
}
