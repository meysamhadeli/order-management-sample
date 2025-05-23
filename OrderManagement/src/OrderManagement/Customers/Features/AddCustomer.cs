using BuildingBlocks.Core.Event;
using BuildingBlocks.Exception;
using BuildingBlocks.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Customers.Dtos;
using OrderManagement.Customers.Enums;
using OrderManagement.Customers.Exceptions;
using OrderManagement.Customers.Models;
using OrderManagement.Data;
using OrderManagement.Identities.Constants;
using System.Globalization;

namespace OrderManagement.Customers.Features
{
    public class AddCustomerEndpoint : IMinimalEndpoint
    {
        public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapPost(
                    $"{EndpointConfig.BaseApiPath}/customers",
                    async (AddCustomerRequestDto request, IMediator mediator) =>
                    {
                        var result = await mediator.Send(
                                         new AddCustomerCommand(
                                             request.FirstName,
                                             request.LastName,
                                             request.Email,
                                             request.UserId,
                                             request.role,
                                             request.InitialBalance));

                        return Results.Created(
                            $"{EndpointConfig.BaseApiPath}/customers/{result.Id}",
                            result);
                    })
                .RequireAuthorization(policy => policy.RequireRole(IdentityConstant.Role.Admin))
                .WithApiVersionSet(builder.NewApiVersionSet("Customer").Build())
                .WithTags("Customer")
                .WithName("AddCustomer")
                .WithOpenApi()
                .HasApiVersion(1.0);

            return builder;
        }
    }

    public record AddCustomerCommand(
        string FirstName,
        string LastName,
        string Email,
        string UserId,
        Role role,
        decimal InitialBalance = 0
    ) : IRequest<CustomerDto>;

    public record AddCustomerRequestDto(
        string FirstName,
        string LastName,
        string Email,
        string UserId,
        Role role = Role.User,
        decimal InitialBalance = 0
    );

    public record CustomerCreatedDomainEvent(
        Guid CustomerId,
        string FirstName,
        string LastName,
        string Email,
        string UserId,
        decimal InitialBalance,
        bool IsDeleted
    ) : IDomainEvent;

    public class AddCustomerHandler : IRequestHandler<AddCustomerCommand, CustomerDto>
    {
        private readonly AppDbContext _dbContext;

        public AddCustomerHandler(
            AppDbContext dbContext,
            ICurrentUserProvider currentUserProvider
        )
        {
            _dbContext = dbContext;
        }

        public async Task<CustomerDto> Handle(
            AddCustomerCommand request,
            CancellationToken cancellationToken
        )
        {
            // Fetch user from database
            var user = await _dbContext.Users
                           .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new UserNotFoundException(request.UserId);
            }

            // Assign role directly via UserRoles table if not already assigned
            var roleName = request.role.ToString().ToLower(CultureInfo.CurrentCulture);

            var role = await _dbContext.Roles
                           .FirstOrDefaultAsync(
                               r => r.NormalizedName == roleName.ToUpper(CultureInfo.InvariantCulture),
                               cancellationToken);

            if (role == null)
            {
                throw new DomainException($"Role '{roleName}' not found.");
            }

            var roleAlreadyAssigned = await _dbContext.UserRoles
                                          .AnyAsync(
                                              ur => ur.UserId == request.UserId &&
                                                    ur.RoleId == role.Id,
                                              cancellationToken);

            if (!roleAlreadyAssigned)
            {
                _dbContext.UserRoles.Add(
                    new IdentityUserRole<string>
                    {
                        UserId = request.UserId,
                        RoleId = role.Id
                    });
            }

            // Check if customer already exists
            var existingCustomer = await _dbContext.Customers
                                       .FirstOrDefaultAsync(
                                           c => c.UserId == request.UserId,
                                           cancellationToken);

            if (existingCustomer != null)
            {
                throw new CustomerAlreadyExistException(existingCustomer.UserId);
            }

            // Check if email already exists
            var emailExists = await _dbContext.Customers
                                  .AnyAsync(c => c.Email == request.Email, cancellationToken);

            if (emailExists)
            {
                throw new EmailAlreadyExistException(request.Email);
            }

            var customer = Customer.Create(
                Guid.NewGuid(),
                request.FirstName,
                request.LastName,
                request.Email,
                user,
                request.InitialBalance);

            await _dbContext.Customers.AddAsync(customer, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CustomerDto(
                customer.Id,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.WalletBalance,
                customer.UserId,
                roleName);
        }
    }

    public class AddCustomerValidator : AbstractValidator<AddCustomerCommand>
    {
        public AddCustomerValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("First name is required and must be less than 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Last name is required and must be less than 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Valid email is required");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.InitialBalance)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Initial balance cannot be negative");
        }
    }
}
