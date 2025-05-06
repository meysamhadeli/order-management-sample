using OrderManagement.Orders.Dtos;
using OrderManagement.Orders.Models;

namespace OrderManagement.Orders;

public static class OrderMappings
{
    public static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.OrderDate,
            order.Status.ToString(),
            order.TotalAmount,
            order.OrderItems.Select(item => new OrderItemDto(
                                        item.Id,
                                        item.Product,
                                        item.UnitPrice,
                                        item.Quantity,
                                        item.TotalPrice)).ToList());
    }
}
