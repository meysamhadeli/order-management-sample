using BuildingBlocks.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Data;
using OrderManagement.Identities.Constants;

namespace YourNamespace.Features.Customers
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

    public record GetCustomerByIdQuery(Guid CustomerId) : IRequest<CustomerResponseDto>;

    public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, CustomerResponseDto>
    {
        private readonly AppDbContext _dbContext;
        private readonly ICurrentUserProvider _currentUserProvider;

        public GetCustomerByIdHandler(
            AppDbContext dbContext,
            ICurrentUserProvider currentUserProvider)
        {
            _dbContext = dbContext;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<CustomerResponseDto> Handle(
            GetCustomerByIdQuery request,
            CancellationToken cancellationToken)
        {
            var customer = await _dbContext.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (customer is null)
            {
                throw new KeyNotFoundException("Customer not found");
            }

            if (!_currentUserProvider.IsAdmin() &&
                customer.UserId != _currentUserProvider.GetCurrentUserId())
            {
                throw new UnauthorizedAccessException("Unauthorized access to customer data");
            }

            return new CustomerResponseDto(
                customer.Id,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.WalletBalance);
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
