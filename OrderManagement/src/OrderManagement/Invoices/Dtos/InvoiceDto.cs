namespace OrderManagement.Invoices.Dtos;

public record InvoiceDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    DateTime DueDate,
    string Status,
    OrderDetailsDto OrderDto);

public record OrderDetailsDto(
    Guid Id,
    DateTime OrderDate,
    string OrderStatus,
    decimal TotalAmount,
    List<OrderItemDetailsDto> ItemsDto);

public record OrderItemDetailsDto(
    Guid Id,
    string Product,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);
