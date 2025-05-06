using BuildingBlocks.Exception;

namespace OrderManagement.Customers.Exceptions;

public class InvalidAmountException : BadRequestException
{
    public InvalidAmountException()
        : base("Amount must be positive or greater than 0.")
    {
    }
}

