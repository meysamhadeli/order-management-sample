namespace OrderManagement.Customers.Dtos;


public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    decimal WalletBalance,
    string UserId,
    string Role);
