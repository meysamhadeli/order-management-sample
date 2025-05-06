using BuildingBlocks.Exception;

namespace OrderManagement.Customers.Exceptions;


public class CustomerAlreadyExistException: ConflictException
{
    public CustomerAlreadyExistException(string userId) :
        base($"Customer already exists for this userId {userId}")
    {
    }
}


