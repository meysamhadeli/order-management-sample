using BuildingBlocks.Exception;

namespace OrderManagement.Customers.Exceptions;

public class CustomerNotFoundException: NotFoundException
{
    public CustomerNotFoundException(Guid customerId) :
        base($"Customer not found with this customerId {customerId}")
    {
    }
}

