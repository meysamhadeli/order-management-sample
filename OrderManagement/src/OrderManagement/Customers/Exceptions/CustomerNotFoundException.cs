using BuildingBlocks.Exception;

namespace OrderManagement.Customers.Exceptions;

public class CustomerNotFoundException : NotFoundException
{
    public CustomerNotFoundException()
        : base("Customer not found.")
    {
    }
}
