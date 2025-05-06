using BuildingBlocks.Exception;

namespace OrderManagement.Invoices.Exceptions;


public class InvoiceNotFoundException : NotFoundException
{
    public InvoiceNotFoundException(Guid invoiceId) :
        base($"Invoice not found with this invoiceId {invoiceId}")
    {
    }
}

