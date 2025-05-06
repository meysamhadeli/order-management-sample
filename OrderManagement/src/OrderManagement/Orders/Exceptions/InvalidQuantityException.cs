using BuildingBlocks.Exception;

namespace OrderManagement.Orders.Exceptions;


public class InvalidQuantityException : BadRequestException
{
    public InvalidQuantityException() :
        base("Quantity must be positive or greater than 0")
    {
    }
}

