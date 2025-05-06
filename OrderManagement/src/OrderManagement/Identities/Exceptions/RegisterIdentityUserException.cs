using BuildingBlocks.Exception;

namespace OrderManagement.Identities.Exceptions;

public class RegisterIdentityUserException : AppException
{
    public RegisterIdentityUserException(string error) : base(error)
    {
    }
}
