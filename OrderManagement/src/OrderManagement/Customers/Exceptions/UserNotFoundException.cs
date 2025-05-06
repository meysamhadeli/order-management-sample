using BuildingBlocks.Exception;

namespace OrderManagement.Customers.Exceptions;

public class UserNotFoundException: NotFoundException
{
    public UserNotFoundException(string userId) :
        base($"User not found with this userId {userId}")
    {
    }
}
