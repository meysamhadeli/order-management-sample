using BuildingBlocks.Core.Model;
using BuildingBlocks.Exception;
using Microsoft.AspNetCore.Identity;
using OrderManagement.Customers.Exceptions;
using OrderManagement.Customers.Features;

namespace OrderManagement.Customers.Models;

public record Customer : Aggregate<Guid>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string UserId { get; init; }
    public IdentityUser? User { get; init; }
    public decimal WalletBalance { get; init; }

    // Private constructor for EF Core
    private Customer() { }

    // Factory method
    public static Customer Create(
        Guid id,
        string firstName,
        string lastName,
        string email,
        IdentityUser user,
        decimal initialBalance = 0)
    {
        if (initialBalance < 0)
            throw new InvalidBalanceException();

        var customer =  new Customer
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserId = user.Id,
            User = user,
            WalletBalance = initialBalance
        };

        // Raise domain event
        customer.AddDomainEvent(new CustomerCreatedDomainEvent(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.UserId,
            customer.WalletBalance,
            customer.IsDeleted));

        return customer;
    }
}
