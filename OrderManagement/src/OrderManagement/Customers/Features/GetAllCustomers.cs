using BuildingBlocks.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<IdentityUser> _userManager;

    public GetAllCustomersHandler(
        AppDbContext dbContext,
        ICurrentUserProvider currentUserProvider,
        UserManager<IdentityUser> userManager
    )
    {
        _dbContext = dbContext;
        _currentUserProvider = currentUserProvider;
        _userManager = userManager;
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

        // Get the list of customers from the database
        var customers = await query.ToListAsync(cancellationToken);

        // Get the user IDs of the customers we fetched
        var userIds = customers.Select(c => c.UserId).ToList();

        // Fetch the roles for all users concurrently
        var roles = await GetRolesForUsers(userIds, cancellationToken);

        // Map the customer information with the roles
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
        // Create a list of tasks to fetch roles for each user
        var roleTasks = userIds
            .Select(userId => _userManager.GetRolesAsync(new IdentityUser {Id = userId}))
            .ToList();

        // Run all tasks concurrently
        var roleResults = await Task.WhenAll(roleTasks);

        // Map the results to a dictionary (UserId -> List of Roles)
        var rolesDict = userIds.Zip(roleResults, (userId, roles) => new {userId, roles})
            .ToDictionary(x => x.userId, x => x.roles.ToList());

        return rolesDict;
    }
}
