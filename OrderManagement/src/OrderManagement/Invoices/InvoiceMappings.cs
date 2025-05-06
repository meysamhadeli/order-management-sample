using OrderManagement.Invoices.Dtos;
using OrderManagement.Invoices.Models;
using OrderManagement.Orders.Models;

namespace OrderManagement.Invoices;

public static class InvoiceMappings
{

    public static InvoiceDto MapToInvoiceDto(Invoice invoice)
    {
        return new InvoiceDto(
            invoice.Id,
            invoice.OrderId,
            invoice.Amount,
            invoice.DueDate,
            invoice.Status.ToString(),
            MapOrderDetailsDto(invoice.Order));
    }

    public static OrderDetailsDto MapOrderDetailsDto(Order order)
    {
        return new OrderDetailsDto(
            order.Id,
            order.OrderDate,
            order.Status.ToString(),
            order.TotalAmount,
            order.OrderItems.Select(item => new OrderItemDetailsDto(
                                        item.Id,
                                        item.Product,
                                        item.UnitPrice,
                                        item.Quantity,
                                        item.TotalPrice)).ToList());
    }
}
