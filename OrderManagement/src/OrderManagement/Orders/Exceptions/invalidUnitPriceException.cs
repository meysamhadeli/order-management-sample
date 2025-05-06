using BuildingBlocks.Exception;

namespace OrderManagement.Orders.Exceptions;

public class invalidUnitPriceException : BadRequestException
{
    public invalidUnitPriceException() :
        base("Unit price must be positive or greater than 0")
    {
    }
}


