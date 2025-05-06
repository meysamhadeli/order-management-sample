using BuildingBlocks.Exception;

namespace OrderManagement.Invoices.Exceptions;


public class OrderNotFoundException : NotFoundException
{
    public OrderNotFoundException(Guid orderId) :
        base($"Order not found with this orderId {orderId}")
    {
    }
}

