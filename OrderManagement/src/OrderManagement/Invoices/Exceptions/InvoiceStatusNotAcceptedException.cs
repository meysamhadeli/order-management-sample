using BuildingBlocks.Exception;
using OrderManagement.Invoices.Enums;

namespace OrderManagement.Invoices.Exceptions;

public class InvoiceStatusNotAcceptedException : BadRequestException
{
    public InvoiceStatusNotAcceptedException(InvoiceStatus status) :
        base($"Cannot pay invoice with status {status}")
    {
    }
}

