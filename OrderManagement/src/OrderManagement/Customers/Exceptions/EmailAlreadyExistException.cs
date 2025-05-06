using BuildingBlocks.Exception;

namespace OrderManagement.Customers.Exceptions;

public class EmailAlreadyExistException: ConflictException
{
    public EmailAlreadyExistException(string email) :
        base($"Email already exists for this email {email}")
    {
    }
}


