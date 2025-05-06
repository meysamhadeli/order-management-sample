using BuildingBlocks.Core.Model;
using BuildingBlocks.Exception;
using OrderManagement.Invoices.Enums;
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

    // Factory method
    public static Invoice Create(
        Guid id,
        Order order,
        decimal amount,
        DateTime dueDate)
    {
        if (amount <= 0)
            throw new DomainException("Invoice amount must be positive");

        if (dueDate < DateTime.UtcNow.Date)
            throw new DomainException("Due date cannot be in the past");

        return new Invoice
               {
                   Id = id,
                   OrderId = order.Id,
                   Order = order,
                   Amount = amount,
                   DueDate = dueDate,
                   Status = InvoiceStatus.Pending
               };
    }

    // Domain behaviors
    public Invoice MarkAsPaid()
    {
        if (Status == InvoiceStatus.Paid)
            throw new DomainException("Invoice already paid");

        return this with { Status = InvoiceStatus.Paid };
    }

    public Invoice MarkAsOverdue()
    {
        if (Status != InvoiceStatus.Pending)
            throw new DomainException("Only pending invoices can be marked overdue");

        if (DueDate >= DateTime.UtcNow.Date)
            throw new DomainException("Invoice is not yet due");

        return this with { Status = InvoiceStatus.Overdue };
    }

    public Invoice Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new DomainException("Paid invoices cannot be cancelled");

        return this with { Status = InvoiceStatus.Cancelled };
    }
}
