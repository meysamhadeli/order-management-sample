using BuildingBlocks.Exception;

namespace OrderManagement.Orders.Exceptions;


public class InvalidOrderItemException : BadRequestException
{
    public InvalidOrderItemException() :
        base("Order must have at least one item")
    {
    }
}

