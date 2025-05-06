using BuildingBlocks.Exception;

namespace OrderManagement.Customers.Exceptions;

public class InvalidBalanceException : BadRequestException
{
    public InvalidBalanceException() :
        base("Initial balance cannot be negative")
    {
    }
}

