using BuildingBlocks.Core.Model;
using BuildingBlocks.Exception;
using Microsoft.AspNetCore.Identity;

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
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required");

        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");

        if (initialBalance < 0)
            throw new DomainException("Initial balance cannot be negative");

        return new Customer
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserId = user.Id,
            User = user,
            WalletBalance = initialBalance
        };
    }

    // Domain behaviors
    public Customer AddFunds(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive");

        return this with { WalletBalance = WalletBalance + amount };
    }

    public Customer DeductFunds(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive");

        if (WalletBalance < amount)
            throw new DomainException("Insufficient funds");

        return this with { WalletBalance = WalletBalance - amount };
    }

    public Customer UpdateEmail(string newEmail)
    {
        if (!IsValidEmail(newEmail))
            throw new DomainException("Invalid email format");

        return this with { Email = newEmail };
    }

    private static bool IsValidEmail(string email)
        => !string.IsNullOrWhiteSpace(email) && email.Contains('@', StringComparison.Ordinal);
}
