using BuildingBlocks.Core.Model;
using OrderManagement.Customers.Models;
using OrderManagement.Invoices.Enums;
using OrderManagement.Invoices.Exceptions;
using OrderManagement.Orders.Enums;
using OrderManagement.Orders.Models;

namespace OrderManagement.Invoices.Models;

public record Invoice : Aggregate<Guid>
{
    public required Guid OrderId { get; init; }
    public Order? Order { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime DueDate { get; init; }
    public required InvoiceStatus Status { get; init; } = InvoiceStatus.Pending;

    // Private constructor for EF Core
    private Invoice() { }

    public static Invoice Create(
        Guid id,
        Guid orderId,
        decimal amount,
        DateTime dueDate)
    {
        if (amount <= 0)
            throw new InvalidInvoiceException();

        if (dueDate <= DateTime.UtcNow)
            throw new InvalidDueDateException();

        return new Invoice
        {
            Id = id,
            OrderId = orderId,
            Amount = amount,
            DueDate = dueDate,
            Status = InvoiceStatus.Pending
        };
    }

    public Invoice ProcessPayment(Customer customer)
    {
        if (Status != InvoiceStatus.Pending)
            throw new InvoiceStatusNotAcceptedException(Status);

        if (customer.WalletBalance < Amount)
            throw new PaymentException(Amount, customer.WalletBalance);

        var updatedCustomer = customer with { WalletBalance = customer.WalletBalance - Amount };

        // Update the Order with the updated Customer and change its status to "Procced"
        var updatedOrder = Order with
        {
            Customer = updatedCustomer,
            Status = OrderStatus.Procced // Set Order status to "Procced"
        };

        // Create a new Invoice with the updated Order and Status
        return this with
        {
            Status = InvoiceStatus.Paid,
            Order = updatedOrder // Link the updated Order
        };
    }
}
