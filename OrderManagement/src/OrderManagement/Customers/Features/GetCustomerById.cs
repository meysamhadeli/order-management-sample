using BuildingBlocks.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Customers.Dtos;
using OrderManagement.Customers.Exceptions;
using OrderManagement.Data;

namespace OrderManagement.Customers.Features
{
    public class GetCustomerByIdEndpoint : IMinimalEndpoint
    {
        public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet(
                $"{EndpointConfig.BaseApiPath}/customers/{{id:guid}}",
                async (Guid id, IMediator mediator) =>
                {
                    var result = await mediator.Send(new GetCustomerByIdQuery(id));
                    return Results.Ok(result);
                })
                .RequireAuthorization()
                .WithApiVersionSet(builder.NewApiVersionSet("Customer").Build())
                .WithTags("Customer")
                .WithName("GetCustomerById")
                .WithOpenApi()
                .HasApiVersion(1.0);

            return builder;
        }
    }

    public record GetCustomerByIdQuery(Guid CustomerId) : IRequest<CustomerDto>;

    public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
    {
        private readonly AppDbContext _dbContext;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly UserManager<IdentityUser> _userManager;

        public GetCustomerByIdHandler(
            AppDbContext dbContext,
            ICurrentUserProvider currentUserProvider,
            UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _currentUserProvider = currentUserProvider;
            _userManager = userManager;
        }

        public async Task<CustomerDto> Handle(
            GetCustomerByIdQuery request,
            CancellationToken cancellationToken)
        {
            var customer = await _dbContext.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (customer is null)
            {
                throw new CustomerNotFoundException(request.CustomerId);
            }

            if (!_currentUserProvider.IsAdmin() && customer.UserId != _currentUserProvider.GetCurrentUserId())
            {
                throw new UnauthorizedAccessException("Unauthorized access to customer data");
            }

            var role = await _userManager.GetRolesAsync((await _userManager.FindByIdAsync(customer.UserId))!);

            return new CustomerDto(
                customer.Id,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.WalletBalance,
                customer.UserId,
                role?.First() ?? string.Empty
                );
        }
    }

    public class GetCustomerByIdValidator : AbstractValidator<GetCustomerByIdQuery>
    {
        public GetCustomerByIdValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("Customer ID is required");
        }
    }
}
