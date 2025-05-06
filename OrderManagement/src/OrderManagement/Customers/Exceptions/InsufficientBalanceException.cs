using BuildingBlocks.Exception;

namespace OrderManagement.Customers.Exceptions;

public class InsufficientBalanceException : BadRequestException
{
    public InsufficientBalanceException()
        : base("Insufficient wallet balance.")
    {
    }
}
