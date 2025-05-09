using BuildingBlocks.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Customers.Dtos;
using OrderManagement.Data;

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
            .RequireAuthorization()
            .WithApiVersionSet(builder.NewApiVersionSet("Customer").Build())
            .WithTags("Customer")
            .WithName("GetAllCustomers")
            .WithOpenApi()
            .HasApiVersion(1.0);

        return builder;
    }
}

public record GetAllCustomersQuery : IRequest<List<CustomerDto>>;

public class GetAllCustomersHandler : IRequestHandler<GetAllCustomersQuery, List<CustomerDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetAllCustomersHandler(
        AppDbContext dbContext,
        ICurrentUserProvider currentUserProvider
    )
    {
        _dbContext = dbContext;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<CustomerDto>> Handle(
        GetAllCustomersQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = _dbContext.Customers.AsNoTracking();

        if (!_currentUserProvider.IsAdmin())
        {
            query = query.Where(c => c.UserId == _currentUserProvider.GetCurrentUserId());
        }

        var customers = await query.ToListAsync(cancellationToken);
        var userIds = customers.Select(c => c.UserId).ToList();

        var roles = await GetRolesForUsers(userIds, cancellationToken);

        var customerDtos = customers.Select(c => new CustomerDto(
                                                c.Id,
                                                c.FirstName,
                                                c.LastName,
                                                c.Email,
                                                c.WalletBalance,
                                                c.UserId,
                                                roles.TryGetValue(c.UserId, out var roleList) ?
                                                    string.Join(", ", roleList) :
                                                    "No Role"))
            .ToList();

        return customerDtos;
    }

    private async Task<Dictionary<string, List<string>>> GetRolesForUsers(
        List<string> userIds,
        CancellationToken cancellationToken
    )
    {
        var roles = await (
                              from userRole in _dbContext.UserRoles
                              join role in _dbContext.Roles on userRole.RoleId equals role.Id
                              where userIds.Contains(userRole.UserId)
                              select new {userRole.UserId, role.Name}
                          ).ToListAsync(cancellationToken);

        var rolesDict = roles
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Name).ToList());

        return rolesDict;
    }
}
