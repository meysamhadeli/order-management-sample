using BuildingBlocks.Exception;

namespace OrderManagement.Invoices.Exceptions;


public class InvalidDueDateException : BadRequestException
{
    public InvalidDueDateException() :
        base("Due date must be in the future")
    {
    }
}

