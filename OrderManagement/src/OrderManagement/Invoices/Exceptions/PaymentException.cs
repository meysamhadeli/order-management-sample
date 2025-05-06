using BuildingBlocks.Exception;

namespace OrderManagement.Invoices.Exceptions;

public class PaymentException : BadRequestException
{
    public PaymentException(decimal amount, decimal walletBalance) :
        base($"Insufficient funds. " + $"Required: {amount}, Available: {walletBalance}")
    {
    }
}
