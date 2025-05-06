using BuildingBlocks.Exception;
using BuildingBlocks.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Customers.Models;
using OrderManagement.Data;
using OrderManagement.Identities.Constants;

namespace YourNamespace.Features.Customers
{
    public class AddCustomerEndpoint : IMinimalEndpoint
    {
        public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapPost(
                $"{EndpointConfig.BaseApiPath}/customers",
                async (AddCustomerRequestDto request, IMediator mediator) =>
                {
                    var result = await mediator.Send(new AddCustomerCommand(
                        request.FirstName,
                        request.LastName,
                        request.Email,
                        request.UserId,
                        request.InitialBalance));

                    return Results.Created($"{EndpointConfig.BaseApiPath}/customers/{result.Id}", result);
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
        decimal InitialBalance = 0) : IRequest<CustomerResponseDto>;

    public record AddCustomerRequestDto(
        string FirstName,
        string LastName,
        string Email,
        string UserId,
        decimal InitialBalance = 0);

    public class AddCustomerHandler : IRequestHandler<AddCustomerCommand, CustomerResponseDto>
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICurrentUserProvider _currentUserProvide;

        public AddCustomerHandler(
            AppDbContext dbContext,
            UserManager<IdentityUser> userManager,
            ICurrentUserProvider currentUserProvide)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _currentUserProvide = currentUserProvide;
        }

        public async Task<CustomerResponseDto> Handle(
            AddCustomerCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUserProvide.IsAdmin())
            {
                throw new ForbiddenException("Unauthorized to create order for this customer");
            }

            // Check if user exists
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // Check if customer already exists for this user
            var existingCustomer = await _dbContext.Customers
                                       .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (existingCustomer != null)
            {
                throw new InvalidOperationException("Customer already exists for this user");
            }

            // Check if email is already in use
            var emailExists = await _dbContext.Customers
                                  .AnyAsync(c => c.Email == request.Email, cancellationToken);

            if (emailExists)
            {
                throw new InvalidOperationException("Email already in use by another customer");
            }

            // Create new customer
            var customer = Customer.Create(
                Guid.NewGuid(),
                request.FirstName,
                request.LastName,
                request.Email,
                user,
                request.InitialBalance);

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CustomerResponseDto(
                customer.Id,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.WalletBalance);
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
