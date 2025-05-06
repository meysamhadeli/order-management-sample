namespace OrderManagement.Orders.Dtos;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    DateTime OrderDate,
    string Status,
    decimal TotalAmount,
    List<OrderItemDto> Items);

public record OrderItemDto(
    Guid Id,
    string Product,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);
