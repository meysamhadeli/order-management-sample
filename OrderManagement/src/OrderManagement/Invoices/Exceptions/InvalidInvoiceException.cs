using BuildingBlocks.Exception;

namespace OrderManagement.Invoices.Exceptions;


public class InvalidInvoiceException : BadRequestException
{
    public InvalidInvoiceException() :
        base("Invoice amount must be positive or greater than 0")
    {
    }
}

