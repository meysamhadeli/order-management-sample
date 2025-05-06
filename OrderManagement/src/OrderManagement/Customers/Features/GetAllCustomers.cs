using BuildingBlocks.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Data;
using OrderManagement.Identities.Constants;

namespace YourNamespace.Features.Customers
{
    public class GetAllCustomersEndpoint : IMinimalEndpoint
    {
        public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet(
                $"{EndpointConfig.BaseApiPath}/customers",
                async (IMediator mediator) =>
                {
                    var result = await mediator.Send(new GetAllCustomersQuery());
                    return Results.Ok(result);
                })
                .RequireAuthorization(policy => policy.RequireRole(IdentityConstant.Role.Admin))
                .WithApiVersionSet(builder.NewApiVersionSet("Customer").Build())
                .WithTags("Customer")
                .WithName("GetAllCustomers")
                .WithOpenApi()
                .HasApiVersion(1.0);

            return builder;
        }
    }

    public record GetAllCustomersQuery : IRequest<List<CustomerResponseDto>>;

    public record CustomerResponseDto(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        decimal WalletBalance);

    public class GetAllCustomersHandler : IRequestHandler<GetAllCustomersQuery, List<CustomerResponseDto>>
    {
        private readonly AppDbContext _dbContext;
        private readonly ICurrentUserProvider _currentUserProvider;

        public GetAllCustomersHandler(
            AppDbContext dbContext,
            ICurrentUserProvider currentUserProvider)
        {
            _dbContext = dbContext;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<List<CustomerResponseDto>> Handle(
            GetAllCustomersQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Customers.AsNoTracking();

            if (!_currentUserProvider.IsAdmin())
            {
                query = query.Where(c => c.UserId == _currentUserProvider.GetCurrentUserId());
            }

            return await query
                .Select(c => new CustomerResponseDto(
                    c.Id,
                    c.FirstName,
                    c.LastName,
                    c.Email,
                    c.WalletBalance))
                .ToListAsync(cancellationToken);
        }
    }
}
